param([Parameter(Mandatory=$true)][string]$GamePath,
      [Parameter(Mandatory=$true)][string]$BaseLibPath)
$ErrorActionPreference = 'Stop'
$repository = Split-Path $PSScriptRoot -Parent
$runtime = Join-Path ([IO.Path]::GetFullPath($GamePath)) '.errorrelic-multiplayer-tests'
$marker = Join-Path $runtime '.errorrelic-test-runtime'
if ((Test-Path $runtime) -and !(Test-Path $marker)) { throw 'Refusing to overwrite an unowned directory.' }
foreach ($path in @((Join-Path $GamePath 'SlayTheSpire2.exe'),(Join-Path $BaseLibPath 'BaseLib.dll'))) {
    if (!(Test-Path $path)) { throw "Missing dependency: $path" }
}
$utf8 = [Text.UTF8Encoding]::new($false)
[IO.Directory]::CreateDirectory($runtime) | Out-Null
[IO.File]::WriteAllText($marker,"Isolated ERROR Relics test runtime.`n",$utf8)
foreach ($name in @('SlayTheSpire2.exe','SlayTheSpire2.pck','release_info.json','crashpad_handler.exe','crashpad_wer.dll','fmod.dll','fmodstudio.dll','libGodotFmod.windows.template_release.x86_64.dll','libspine_godot.windows.template_release.x86_64.dll','libsentry.windows.release.x86_64.dll')) {
    $target = Join-Path $runtime $name
    if (!(Test-Path $target)) { New-Item -ItemType HardLink -Path $target -Value (Join-Path $GamePath $name) | Out-Null }
}
$dataName = 'data_sts2_windows_x86_64'
if (!(Test-Path (Join-Path $runtime $dataName))) {
    New-Item -ItemType Junction -Path (Join-Path $runtime $dataName) -Value (Join-Path $GamePath $dataName) | Out-Null
}
[IO.File]::WriteAllText((Join-Path $runtime 'override.cfg'),"[application]`nconfig/use_custom_user_dir=true`nconfig/custom_user_dir_name=`"ErrorRelics-MultiplayerTests`"`n",$utf8)
foreach ($id in @('1','1001')) {
    $folder = Join-Path $env:APPDATA "ErrorRelics-MultiplayerTests/default/$id"
    [IO.Directory]::CreateDirectory($folder) | Out-Null
    $file = Join-Path $folder 'settings.save'
    $settings = if (Test-Path $file) { Get-Content $file -Raw | ConvertFrom-Json -AsHashtable } else { @{} }
    $settings['mod_settings'] = @{mods_enabled=$true;mod_list=@()}
    $settings['seen_ea_disclaimer'] = $true
    $settings['language'] = 'zhs'
    $settings['schema_version'] = 8
    $settings['fps_limit'] = 30
    foreach ($key in @('volume_master','volume_bgm','volume_sfx','volume_ambience')) { $settings[$key] = 0.0 }
    [IO.File]::WriteAllText($file,($settings | ConvertTo-Json -Depth 12).Replace("`r`n","`n")+"`n",$utf8)
}
foreach ($mod in @('BaseLib','ErrorRelics','ErrorRelics.Tests')) {
    [IO.Directory]::CreateDirectory((Join-Path $runtime "mods/$mod")) | Out-Null
}
foreach ($extension in @('dll','pck','json')) {
    Copy-Item (Join-Path $BaseLibPath "BaseLib.$extension") -Destination (Join-Path $runtime 'mods/BaseLib')
}
[IO.File]::WriteAllText((Join-Path $runtime 'mods/ErrorRelics.Tests/ErrorRelics.Tests.json'),'{"id":"ErrorRelics.Tests","name":"ERROR multiplayer test driver","author":"Local testing","version":"0.1.0","min_game_version":"0.111.0","has_dll":true,"has_pck":false,"affects_gameplay":true,"dependencies":[{"id":"ErrorRelics","min_version":"0.91.0"}]}' + "`n",$utf8)
$pack = Join-Path $runtime 'mods/ErrorRelics/ErrorRelics.pck'
$arguments = '--headless --audio-driver Dummy --path "'+$runtime+'" --script "'+(Join-Path $PSScriptRoot 'pack-assets.gd')+'" -- "'+$repository+'" "'+$pack+'"'
$p = Start-Process -FilePath (Join-Path $runtime 'SlayTheSpire2.exe') -ArgumentList $arguments -WorkingDirectory $runtime -WindowStyle Hidden -PassThru -RedirectStandardOutput (Join-Path $runtime 'pack.out.log') -RedirectStandardError (Join-Path $runtime 'pack.err.log')
if (!$p.WaitForExit(60000)) { $p.Kill(); throw 'Asset packer timed out.' }
if ($p.ExitCode -ne 0) { throw 'Asset packer failed; see pack.err.log.' }
Write-Output "Prepared: $runtime"
Write-Output 'Run tools/test.ps1 once, then tools/start-smoke.ps1 with this RuntimePath.'
