param([Parameter(Mandatory=$true)][string]$RuntimePath)
$ErrorActionPreference = 'Stop'
if (!(Test-Path (Join-Path $RuntimePath '.errorrelic-test-runtime'))) {
    throw 'Expected the isolated ERROR test runtime marker.'
}
$repository = Split-Path $PSScriptRoot -Parent
$exe = Join-Path $RuntimePath 'SlayTheSpire2.exe'
Get-Process SlayTheSpire2 -ErrorAction SilentlyContinue | Where-Object { $_.Path -eq $exe } | Stop-Process
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
$hostArgs = '--headless --audio-driver Dummy --path "'+$RuntimePath+'" --force-steam off --log-file "'+$RuntimePath+'/host-godot.log" -fastmp host_standard --errorrelic-smoke'
$hostProcess = Start-Process -FilePath $exe -ArgumentList $hostArgs -WorkingDirectory $RuntimePath -WindowStyle Hidden -PassThru -RedirectStandardOutput (Join-Path $RuntimePath 'host.out.log') -RedirectStandardError (Join-Path $RuntimePath 'host.err.log')
$deadline = [DateTime]::UtcNow.AddSeconds(20)
while ([DateTime]::UtcNow -lt $deadline) {
    if ((Get-Content (Join-Path $RuntimePath 'host.out.log') -Raw -ErrorAction SilentlyContinue) -match "Finished mod initialization for 'BaseLib'") { break }
    Start-Sleep -Milliseconds 250
}
$clientArgs = '--headless --audio-driver Dummy --path "'+$RuntimePath+'" --force-steam off --log-file "'+$RuntimePath+'/client-godot.log" -fastmp join -clientId 1001 --errorrelic-smoke'
$clientProcess = Start-Process -FilePath $exe -ArgumentList $clientArgs -WorkingDirectory $RuntimePath -WindowStyle Hidden -PassThru -RedirectStandardOutput (Join-Path $RuntimePath 'client.out.log') -RedirectStandardError (Join-Path $RuntimePath 'client.err.log')
[pscustomobject]@{HostPid=$hostProcess.Id;ClientPid=$clientProcess.Id;RuntimePath=$RuntimePath}
