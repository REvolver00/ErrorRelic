param([string]$GamePath)
$ErrorActionPreference = 'Stop'
if (!$GamePath) {
    $GamePath = (Get-ItemProperty 'HKLM:\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\Steam App 2868840').InstallLocation
}
$repository = Split-Path $PSScriptRoot -Parent
$data = Join-Path $GamePath 'data_sts2_windows_x86_64'
$runtime = Get-Content (Join-Path $data 'sts2.runtimeconfig.json') -Raw | ConvertFrom-Json
$version = $runtime.runtimeOptions.includedFrameworks[0].version
$output = Join-Path $repository '.godot/errorrelic-tests'
dotnet publish (Join-Path $repository 'tests/ErrorRelics.Tests.csproj') -c Debug -r win-x64 --self-contained true "-p:RuntimeFrameworkVersion=$version" "-p:Sts2Path=$GamePath" -p:DeployModOnBuild=false -p:UpdateDependencyManifest=false -o $output -v:q
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
& (Join-Path $output 'ErrorRelics.Tests.exe') $data
exit $LASTEXITCODE
