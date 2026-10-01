param([Parameter(Mandatory=$true)][string]$RuntimePath,
      [ValidateSet('single','host-owner','client-owner')][string[]]$Cases = @('single','host-owner','client-owner'))
$ErrorActionPreference = 'Stop'
if (!(Test-Path (Join-Path $RuntimePath '.errorrelic-test-runtime'))) {
    throw 'Expected the isolated ERROR test runtime marker.'
}
$repository = Split-Path $PSScriptRoot -Parent
$output = Join-Path $repository '.godot/errorrelic-tests'
dotnet publish (Join-Path $repository 'tests/ErrorRelics.Tests.csproj') --no-restore -c Debug -r win-x64 --self-contained true -p:RuntimeFrameworkVersion=9.0.7 -p:DeployModOnBuild=false -p:UpdateDependencyManifest=false -o $output -v:q -clp:ErrorsOnly
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
Copy-Item (Join-Path $output 'ErrorRelics.Tests.dll') -Destination (Join-Path $RuntimePath 'mods/ErrorRelics.Tests')
Copy-Item (Join-Path $repository '.godot/mono/temp/bin/Debug/ErrorRelics.dll'),(Join-Path $repository 'ErrorRelics.json') -Destination (Join-Path $RuntimePath 'mods/ErrorRelics')
foreach ($id in @('1','1001')) {
    $path = Join-Path $env:APPDATA "ErrorRelics-MultiplayerTests/default/$id/settings.save"
    $settings = Get-Content $path -Raw | ConvertFrom-Json
    foreach ($key in @('volume_master','volume_bgm','volume_sfx','volume_ambience')) { $settings.$key = 0.0 }
    [IO.File]::WriteAllText($path,($settings | ConvertTo-Json -Depth 12).Replace("`r`n","`n")+"`n",[Text.UTF8Encoding]::new($false))
}
foreach ($case in $Cases) {
    $roles = if ($case -eq 'single') { @('single') } else { @('host','client') }
    $processes = @()
    try {
        foreach ($role in $roles) {
            $mode = if ($role -eq 'host') { '--rewards-mp -fastmp host_standard' } elseif ($role -eq 'client') { '--rewards-mp -fastmp join -clientId 1001' } else { '' }
            $owner = if ($case -eq 'client-owner') { '--reward-owner-client' } else { '' }
            $prefix = "room-rewards-$case-$role"
            $arguments = '--headless --audio-driver Dummy --path "'+$RuntimePath+'" --force-steam off --errorrelic-room-rewards-smoke '+$mode+' '+$owner+' --log-file "'+$RuntimePath+'/'+$prefix+'-godot.log"'
            $process = Start-Process (Join-Path $RuntimePath 'SlayTheSpire2.exe') -ArgumentList $arguments -WorkingDirectory $RuntimePath -WindowStyle Hidden -PassThru -RedirectStandardOutput (Join-Path $RuntimePath "$prefix.out.log") -RedirectStandardError (Join-Path $RuntimePath "$prefix.err.log")
            $processes += $process
            if ($role -eq 'host') {
                $deadline = [DateTime]::UtcNow.AddSeconds(40)
                while ([DateTime]::UtcNow -lt $deadline) {
                    $log = Get-Content (Join-Path $RuntimePath "$prefix.out.log") -Raw -ErrorAction SilentlyContinue
                    if ($log -match 'main menu loaded \(complete\)') { break }
                    Start-Sleep -Milliseconds 300
                }
            }
        }
        $deadline = [DateTime]::UtcNow.AddSeconds(90)
        $passed = $false
        while ([DateTime]::UtcNow -lt $deadline) {
            $logs = @($roles | ForEach-Object { Get-Content (Join-Path $RuntimePath "room-rewards-$case-$_.out.log") -Raw -ErrorAction SilentlyContinue })
            if (@($logs | Where-Object { $_ -match 'ERROR_ROOM_REWARDS_FAIL' }).Count) { break }
            if (@($logs | Where-Object { $_ -match 'ERROR_ROOM_REWARDS_PASS' }).Count -eq $roles.Count) { $passed = $true; break }
            Start-Sleep -Milliseconds 400
        }
        foreach ($role in $roles) {
            $path = Join-Path $RuntimePath "room-rewards-$case-$role.out.log"
            Select-String -Path $path -Pattern 'ERROR_ROOM_REWARDS_' -Context 0,2 | ForEach-Object { $_.ToString() }
        }
        if (!$passed) { throw "Room rewards failed or timed out: $case" }
    } finally {
        foreach ($process in $processes) { if (!$process.HasExited) { $process.Kill(); $process.WaitForExit() } }
    }
}
