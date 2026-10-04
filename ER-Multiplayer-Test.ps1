[CmdletBinding()]
param(
    [ValidateSet('Menu','AutoSmoke','Gift2P','CheckLogs','Stop')]
    [string]$Mode = 'Menu',
    [string]$GamePath = '',
    [string]$BaseLibPath = '',
    [switch]$Reprepare
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

$Repo = $PSScriptRoot
$Project = Join-Path $Repo 'ErrorRelics.csproj'
$Tools = Join-Path $Repo 'tools'
$ResultsRoot = Join-Path $Repo 'multiplayer-test-results'
$ConfigFile = Join-Path $Repo '.er-mp-test.json'

if (!(Test-Path $Project)) {
    throw "ER-Multiplayer-Test.ps1 must be placed in the ErrorRelics project root (next to ErrorRelics.csproj). Current: $Repo"
}

function Write-Section([string]$Text) {
    Write-Host ''
    Write-Host ('=' * 72) -ForegroundColor DarkCyan
    Write-Host ('  ' + $Text) -ForegroundColor Cyan
    Write-Host ('=' * 72) -ForegroundColor DarkCyan
}

function Load-LocalConfig {
    $cfg = @{}
    if (!(Test-Path $ConfigFile)) { return $cfg }

    try {
        $obj = Get-Content $ConfigFile -Raw | ConvertFrom-Json

        if ($null -ne $obj.PSObject.Properties['GamePath']) {
            $cfg['GamePath'] = [string]$obj.GamePath
        }

        if ($null -ne $obj.PSObject.Properties['BaseLibPath']) {
            $cfg['BaseLibPath'] = [string]$obj.BaseLibPath
        }
    } catch {}

    return $cfg
}

function Save-LocalConfig([string]$ResolvedGamePath, [string]$ResolvedBaseLibPath) {
    $cfg = [ordered]@{
        GamePath = $ResolvedGamePath
        BaseLibPath = $ResolvedBaseLibPath
    }
    $json = $cfg | ConvertTo-Json
    [IO.File]::WriteAllText($ConfigFile, $json + "`n", [Text.UTF8Encoding]::new($false))
}

function Test-GamePath([string]$Path) {
    if (!$Path) { return $false }
    return (Test-Path (Join-Path $Path 'SlayTheSpire2.exe')) -and
           (Test-Path (Join-Path $Path 'data_sts2_windows_x86_64\sts2.dll'))
}

function Find-GamePath {
    param([string]$Requested)

    $cfg = Load-LocalConfig
    $candidates = New-Object System.Collections.Generic.List[string]

    if ($Requested) { $candidates.Add($Requested) }
    if ($cfg.ContainsKey('GamePath') -and $cfg.GamePath) { $candidates.Add([string]$cfg.GamePath) }

    foreach ($key in @(
        'HKLM:\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\Steam App 2868840',
        'HKLM:\SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall\Steam App 2868840'
    )) {
        try {
            $p = (Get-ItemProperty $key -ErrorAction Stop).InstallLocation
            if ($p) { $candidates.Add([string]$p) }
        } catch {}
    }

    try {
        $steam = (Get-ItemProperty 'HKCU:\Software\Valve\Steam' -ErrorAction Stop).SteamPath
        if ($steam) {
            $candidates.Add((Join-Path $steam 'steamapps\common\Slay the Spire 2'))
            $vdf = Join-Path $steam 'steamapps\libraryfolders.vdf'
            if (Test-Path $vdf) {
                foreach ($line in Get-Content $vdf) {
                    if ($line -match '"path"\s+"([^"]+)"') {
                        $lib = $Matches[1].Replace('\\','\')
                        $candidates.Add((Join-Path $lib 'steamapps\common\Slay the Spire 2'))
                    }
                }
            }
        }
    } catch {}

    foreach ($drive in @('C','D','E','F','G')) {
        $candidates.Add("${drive}:\Program Files (x86)\Steam\steamapps\common\Slay the Spire 2")
        $candidates.Add("${drive}:\SteamLibrary\steamapps\common\Slay the Spire 2")
        $candidates.Add("${drive}:\Steam\steamapps\common\Slay the Spire 2")
    }

    foreach ($candidate in $candidates | Select-Object -Unique) {
        try {
            $full = [IO.Path]::GetFullPath($candidate)
            if (Test-GamePath $full) { return $full }
        } catch {}
    }
    return $null
}

function Test-BaseLibFolder([string]$Folder) {
    if (!$Folder) { return $false }
    return (Test-Path (Join-Path $Folder 'BaseLib.dll')) -and
           (Test-Path (Join-Path $Folder 'BaseLib.json')) -and
           (Test-Path (Join-Path $Folder 'BaseLib.pck'))
}

function Find-BaseLibPath {
    param([string]$Requested, [string]$ResolvedGamePath)

    $cfg = Load-LocalConfig
    $candidates = New-Object System.Collections.Generic.List[string]

    if ($Requested) {
        if (Test-Path $Requested -PathType Leaf) {
            $candidates.Add((Split-Path $Requested -Parent))
        } else {
            $candidates.Add($Requested)
        }
    }

    if ($cfg.ContainsKey('BaseLibPath') -and $cfg.BaseLibPath) {
        $candidates.Add([string]$cfg.BaseLibPath)
    }

    $candidates.Add((Join-Path $ResolvedGamePath 'mods\BaseLib'))

    foreach ($candidate in $candidates | Select-Object -Unique) {
        try {
            $full = [IO.Path]::GetFullPath($candidate)
            if (Test-BaseLibFolder $full) { return $full }
        } catch {}
    }

    # Search the game's mods folder.
    $mods = Join-Path $ResolvedGamePath 'mods'
    if (Test-Path $mods) {
        $dlls = Get-ChildItem $mods -Filter BaseLib.dll -File -Recurse -ErrorAction SilentlyContinue
        foreach ($dll in $dlls) {
            $folder = $dll.Directory.FullName
            if (Test-BaseLibFolder $folder) { return $folder }
        }
    }

    # Search Steam Workshop for STS2. This covers subscribed BaseLib installs
    # that are not copied into <game>\mods.
    try {
        $steamRoots = New-Object System.Collections.Generic.List[string]
        $steam = (Get-ItemProperty 'HKCU:\Software\Valve\Steam' -ErrorAction Stop).SteamPath
        if ($steam) { $steamRoots.Add([string]$steam) }

        # Infer the Steam library from the resolved game path.
        $gameFull = [IO.Path]::GetFullPath($ResolvedGamePath)
        $marker = [IO.Path]::DirectorySeparatorChar + 'steamapps' + [IO.Path]::DirectorySeparatorChar
        $idx = $gameFull.IndexOf($marker, [StringComparison]::OrdinalIgnoreCase)
        if ($idx -ge 0) {
            $steamRoots.Add($gameFull.Substring(0, $idx))
        }

        foreach ($root in $steamRoots | Select-Object -Unique) {
            $workshop = Join-Path $root 'steamapps\workshop\content\2868840'
            if (Test-Path $workshop) {
                $dlls = Get-ChildItem $workshop -Filter BaseLib.dll -File -Recurse -ErrorAction SilentlyContinue
                foreach ($dll in $dlls) {
                    $folder = $dll.Directory.FullName
                    if (Test-BaseLibFolder $folder) { return $folder }
                }
            }
        }
    } catch {}

    # Search the user's NuGet cache. Some BaseLib packages include the runtime
    # mod payload in addition to the compile-time DLL.
    $nuget = Join-Path $env:USERPROFILE '.nuget\packages\alchyr.sts2.baselib'
    if (Test-Path $nuget) {
        $dlls = Get-ChildItem $nuget -Filter BaseLib.dll -File -Recurse -ErrorAction SilentlyContinue |
            Sort-Object FullName -Descending
        foreach ($dll in $dlls) {
            $folder = $dll.Directory.FullName
            if (Test-BaseLibFolder $folder) { return $folder }
        }
    }

    return $null
}

function Resolve-Environment {
    $resolvedGame = Find-GamePath $GamePath
    if (!$resolvedGame) {
        if ($Mode -eq 'Menu') {
            $entered = Read-Host 'Cannot auto-detect Slay the Spire 2. Paste the game folder path'
            $resolvedGame = Find-GamePath $entered
        }
        if (!$resolvedGame) { throw 'Cannot find a valid Slay the Spire 2 installation.' }
    }

    $resolvedBase = Find-BaseLibPath $BaseLibPath $resolvedGame
    if (!$resolvedBase) {
        if ($Mode -eq 'Menu') {
            $entered = Read-Host 'Cannot auto-detect BaseLib. Paste the folder containing BaseLib.dll/BaseLib.json/BaseLib.pck'
            $resolvedBase = Find-BaseLibPath $entered $resolvedGame
        }
        if (!$resolvedBase) { throw 'Cannot find BaseLib.dll + BaseLib.json + BaseLib.pck.' }
    }

    Save-LocalConfig $resolvedGame $resolvedBase
    return [pscustomobject]@{ GamePath=$resolvedGame; BaseLibPath=$resolvedBase }
}

function Stop-TestProcesses([string]$RuntimePath) {
    $exe = Join-Path $RuntimePath 'SlayTheSpire2.exe'
    $stopped = 0
    Get-Process SlayTheSpire2 -ErrorAction SilentlyContinue | ForEach-Object {
        try {
            if ($_.Path -eq $exe) {
                Stop-Process -Id $_.Id -Force -ErrorAction SilentlyContinue
                $stopped++
            }
        } catch {}
    }
    return $stopped
}

function Initialize-IsolatedRuntime([string]$ResolvedGamePath, [string]$ResolvedBaseLibPath, [string]$RuntimePath) {
    $marker = Join-Path $RuntimePath '.errorrelic-test-runtime'

    if ((Test-Path $RuntimePath) -and !(Test-Path $marker)) {
        throw "Refusing to overwrite an unowned directory: $RuntimePath"
    }

    if (!(Test-BaseLibFolder $ResolvedBaseLibPath)) {
        throw "BaseLib runtime payload is incomplete. Need BaseLib.dll, BaseLib.json, BaseLib.pck in: $ResolvedBaseLibPath"
    }

    $requiredGameFiles = @(
        'SlayTheSpire2.exe',
        'SlayTheSpire2.pck',
        'release_info.json',
        'crashpad_handler.exe',
        'crashpad_wer.dll',
        'fmod.dll',
        'fmodstudio.dll',
        'libGodotFmod.windows.template_release.x86_64.dll',
        'libspine_godot.windows.template_release.x86_64.dll',
        'libsentry.windows.release.x86_64.dll'
    )

    New-Item $RuntimePath -ItemType Directory -Force | Out-Null
    [IO.File]::WriteAllText($marker, "Isolated ERROR Relics test runtime.`n", [Text.UTF8Encoding]::new($false))

    foreach ($name in $requiredGameFiles) {
        $source = Join-Path $ResolvedGamePath $name
        if (!(Test-Path $source)) {
            throw "Missing game dependency: $source"
        }

        $target = Join-Path $RuntimePath $name
        if (!(Test-Path $target)) {
            try {
                New-Item -ItemType HardLink -Path $target -Value $source -ErrorAction Stop | Out-Null
            } catch {
                Copy-Item $source -Destination $target -Force
            }
        }
    }

    $dataName = 'data_sts2_windows_x86_64'
    $dataSource = Join-Path $ResolvedGamePath $dataName
    $dataTarget = Join-Path $RuntimePath $dataName
    if (!(Test-Path $dataSource)) {
        throw "Missing game data directory: $dataSource"
    }
    if (!(Test-Path $dataTarget)) {
        try {
            New-Item -ItemType Junction -Path $dataTarget -Value $dataSource -ErrorAction Stop | Out-Null
        } catch {
            throw "Could not create the isolated game-data junction: $($_.Exception.Message)"
        }
    }

    $utf8 = New-Object Text.UTF8Encoding($false)
    [IO.File]::WriteAllText(
        (Join-Path $RuntimePath 'override.cfg'),
        "[application]`nconfig/use_custom_user_dir=true`nconfig/custom_user_dir_name=`"ErrorRelics-MultiplayerTests`"`n",
        $utf8
    )

    foreach ($id in @('1','1001')) {
        $folder = Join-Path $env:APPDATA "ErrorRelics-MultiplayerTests\default\$id"
        New-Item $folder -ItemType Directory -Force | Out-Null

        # Write a deterministic isolated test settings file. This avoids the
        # PowerShell 7-only ConvertFrom-Json -AsHashtable used by the project's
        # original prepare-smoke.ps1.
        $settings = [ordered]@{
            mod_settings = [ordered]@{
                mods_enabled = $true
                mod_list = @()
            }
            seen_ea_disclaimer = $true
            language = 'zhs'
            schema_version = 8
            fps_limit = 30
            volume_master = 0.0
            volume_bgm = 0.0
            volume_sfx = 0.0
            volume_ambience = 0.0
        }

        $json = $settings | ConvertTo-Json -Depth 12
        [IO.File]::WriteAllText(
            (Join-Path $folder 'settings.save'),
            $json.Replace("`r`n","`n") + "`n",
            $utf8
        )
    }

    foreach ($mod in @('BaseLib','ErrorRelics','ErrorRelics.Tests')) {
        New-Item (Join-Path $RuntimePath "mods\$mod") -ItemType Directory -Force | Out-Null
    }

    foreach ($extension in @('dll','pck','json')) {
        Copy-Item (Join-Path $ResolvedBaseLibPath "BaseLib.$extension") `
            -Destination (Join-Path $RuntimePath 'mods\BaseLib') -Force
    }

    [IO.File]::WriteAllText(
        (Join-Path $RuntimePath 'mods\ErrorRelics.Tests\ErrorRelics.Tests.json'),
        '{"id":"ErrorRelics.Tests","name":"ERROR multiplayer test driver","author":"Local testing","version":"0.1.0","min_game_version":"0.111.0","has_dll":true,"has_pck":false,"affects_gameplay":true,"dependencies":[{"id":"ErrorRelics","min_version":"0.91.0"}]}' + "`n",
        $utf8
    )
}

function Ensure-Runtime([string]$ResolvedGamePath, [string]$ResolvedBaseLibPath) {
    $runtime = Join-Path ([IO.Path]::GetFullPath($ResolvedGamePath)) '.errorrelic-multiplayer-tests'
    $marker = Join-Path $runtime '.errorrelic-test-runtime'

    if ($Reprepare -and (Test-Path $runtime)) {
        if (!(Test-Path $marker)) { throw "Refusing to remove unowned directory: $runtime" }
        Stop-TestProcesses $runtime | Out-Null
        Remove-Item $runtime -Recurse -Force
    }

    if (!(Test-Path $marker)) {
        Write-Host 'Preparing isolated runtime and isolated save directory...' -ForegroundColor Yellow
        Initialize-IsolatedRuntime $ResolvedGamePath $ResolvedBaseLibPath $runtime
    }

    foreach ($ext in @('dll','json','pck')) {
        Copy-Item (Join-Path $ResolvedBaseLibPath "BaseLib.$ext") -Destination (Join-Path $runtime 'mods\BaseLib') -Force
    }
    return $runtime
}

function Get-RuntimeFrameworkVersion([string]$ResolvedGamePath) {
    $runtimeConfig = Join-Path $ResolvedGamePath 'data_sts2_windows_x86_64\sts2.runtimeconfig.json'
    $json = Get-Content $runtimeConfig -Raw | ConvertFrom-Json
    return [string]$json.runtimeOptions.includedFrameworks[0].version
}

function Build-And-DeployER([string]$ResolvedGamePath, [string]$RuntimePath) {
    Write-Host 'Building current ErrorRelics source...' -ForegroundColor Yellow
    & dotnet build $Project -c Debug "-p:Sts2Path=$ResolvedGamePath" -p:DeployModOnBuild=false -p:UpdateDependencyManifest=false -v:q
    if ($LASTEXITCODE -ne 0) { throw 'ErrorRelics build failed.' }

    $dll = Join-Path $Repo '.godot\mono\temp\bin\Debug\ErrorRelics.dll'
    if (!(Test-Path $dll)) {
        $dll = Get-ChildItem $Repo -Filter ErrorRelics.dll -File -Recurse -ErrorAction SilentlyContinue |
            Where-Object { $_.FullName -notmatch '\\mods\\' -and $_.FullName -notmatch '\\multiplayer-test-results\\' } |
            Sort-Object LastWriteTime -Descending |
            Select-Object -First 1 -ExpandProperty FullName
    }
    if (!$dll -or !(Test-Path $dll)) { throw 'Build succeeded but ErrorRelics.dll was not found.' }

    $modDir = Join-Path $RuntimePath 'mods\ErrorRelics'
    New-Item $modDir -ItemType Directory -Force | Out-Null
    Copy-Item $dll -Destination (Join-Path $modDir 'ErrorRelics.dll') -Force
    Copy-Item (Join-Path $Repo 'ErrorRelics.json') -Destination $modDir -Force

    $pdb = [IO.Path]::ChangeExtension($dll, '.pdb')
    if (Test-Path $pdb) { Copy-Item $pdb -Destination $modDir -Force }

    Write-Host 'Repacking ER assets/localization...' -ForegroundColor Yellow
    $pack = Join-Path $modDir 'ErrorRelics.pck'
    $packOut = Join-Path $RuntimePath 'pack-current.out.log'
    $packErr = Join-Path $RuntimePath 'pack-current.err.log'
    Remove-Item $packOut,$packErr,$pack -Force -ErrorAction SilentlyContinue

    # Match the project's own PackErrorRelicsAssetsOnBuild target:
    # use the REAL game executable + REAL game project path to run Godot,
    # while writing the resulting PCK into the isolated multiplayer runtime.
    # This is more reliable than launching the packer from the stripped-down
    # isolated runtime itself.
    $realExe = Join-Path $ResolvedGamePath 'SlayTheSpire2.exe'
    $packScript = Join-Path $Tools 'pack-assets.gd'
    $packArgs = '--headless --audio-driver Dummy --path "'+$ResolvedGamePath+'" --script "'+$packScript+'" -- "'+$Repo+'" "'+$pack+'"'

    $p = Start-Process -FilePath $realExe `
        -ArgumentList $packArgs `
        -WorkingDirectory $ResolvedGamePath `
        -WindowStyle Hidden `
        -PassThru `
        -RedirectStandardOutput $packOut `
        -RedirectStandardError $packErr

    if (!$p.WaitForExit(90000)) {
        try { $p.Kill() } catch {}
        Write-Host ''
        Write-Host '--- pack-current.out.log ---' -ForegroundColor Yellow
        if (Test-Path $packOut) { Get-Content $packOut -Tail 120 }
        Write-Host ''
        Write-Host '--- pack-current.err.log ---' -ForegroundColor Yellow
        if (Test-Path $packErr) { Get-Content $packErr -Tail 120 }
        throw "Asset packer timed out."
    }

    # PowerShell 5.1 may return before redirected stdout/stderr has fully
    # flushed to disk. Wait briefly for BOTH the generated PCK and the explicit
    # success marker instead of sampling them only once.
    $packExists = $false
    $packSizeOk = $false
    $packedMarker = $false

    $validationDeadline = [DateTime]::UtcNow.AddSeconds(8)
    do {
        Start-Sleep -Milliseconds 200

        $packExists = Test-Path $pack
        $packSizeOk = $false
        if ($packExists) {
            try {
                $packSizeOk = ((Get-Item $pack -ErrorAction Stop).Length -gt 0)
            } catch {}
        }

        $packedMarker = $false
        if (Test-Path $packOut) {
            try {
                $outText = [IO.File]::ReadAllText($packOut)
                $packedMarker = $outText.Contains('ERROR_ASSETS_PACKED:')
            } catch {}
        }

        if ($packExists -and $packSizeOk -and $packedMarker) {
            break
        }
    } while ([DateTime]::UtcNow -lt $validationDeadline)

    Write-Host "PCK validation: exists=$packExists nonEmpty=$packSizeOk marker=$packedMarker" -ForegroundColor DarkGray

    if (!$packExists -or !$packSizeOk -or !$packedMarker) {
        Write-Host ''
        Write-Host '--- pack-current.out.log ---' -ForegroundColor Yellow
        if (Test-Path $packOut) { Get-Content $packOut -Tail 120 }
        Write-Host ''
        Write-Host '--- pack-current.err.log ---' -ForegroundColor Yellow
        if (Test-Path $packErr) { Get-Content $packErr -Tail 120 }
        Write-Host ''
        $exitText = '<unavailable>'
        try {
            if ($null -ne $p.ExitCode) { $exitText = [string]$p.ExitCode }
        } catch {}
        throw "Asset packer failed validation (exit code $exitText). exists=$packExists nonEmpty=$packSizeOk marker=$packedMarker"
    }

    Write-Host "PCK created and validated: $pack" -ForegroundColor Green
}

function New-Session([string]$Name) {
    New-Item $ResultsRoot -ItemType Directory -Force | Out-Null
    $stamp = Get-Date -Format 'yyyy-MM-dd_HH-mm-ss'
    $dir = Join-Path $ResultsRoot "$stamp-$Name"
    New-Item $dir -ItemType Directory -Force | Out-Null
    return $dir
}

function Wait-ForText([string]$File, [string]$Pattern, [int]$Seconds) {
    $deadline = [DateTime]::UtcNow.AddSeconds($Seconds)
    while ([DateTime]::UtcNow -lt $deadline) {
        if (Test-Path $File) {
            try {
                if ((Get-Content $File -Raw -ErrorAction SilentlyContinue) -match $Pattern) { return $true }
            } catch {}
        }
        Start-Sleep -Milliseconds 250
    }
    return $false
}

function Write-LogReport([string]$Session, [string]$Title) {
    $report = Join-Path $Session 'log-summary.txt'
    $pattern = '(?i)(Unhandled|NullReferenceException|InvalidOperationException|ArgumentException|StackOverflowException|FATAL|CRITICAL|desync|disconnect|serialization.+fail|deserialize.+fail|Gift relic failed)'
    $lines = New-Object System.Collections.Generic.List[string]
    $lines.Add($Title)
    $lines.Add(('=' * 72))
    $lines.Add("Session: $Session")
    $lines.Add('')

    $hits = 0
    foreach ($file in Get-ChildItem $Session -Filter *.log -File -ErrorAction SilentlyContinue | Sort-Object Name) {
        $matches = @(Select-String -Path $file.FullName -Pattern $pattern -AllMatches -ErrorAction SilentlyContinue)
        $lines.Add("[$($file.Name)] suspicious lines: $($matches.Count)")
        foreach ($m in $matches | Select-Object -First 40) {
            $lines.Add("  L$($m.LineNumber): $($m.Line.Trim())")
            $hits++
        }
        $lines.Add('')
    }
    $lines.Add("Total suspicious lines shown: $hits")
    [IO.File]::WriteAllLines($report, $lines, [Text.UTF8Encoding]::new($false))
    return $report
}

function Run-AutoSmoke([string]$ResolvedGamePath, [string]$ResolvedBaseLibPath) {
    Write-Section 'ERROR Relics - automatic 2P multiplayer smoke test'
    $runtime = Ensure-Runtime $ResolvedGamePath $ResolvedBaseLibPath
    Stop-TestProcesses $runtime | Out-Null
    Build-And-DeployER $ResolvedGamePath $runtime

    $version = Get-RuntimeFrameworkVersion $ResolvedGamePath
    $output = Join-Path $Repo '.godot\errorrelic-tests'
    Write-Host "Publishing test-only driver for game .NET runtime $version..." -ForegroundColor Yellow
    & dotnet publish (Join-Path $Repo 'tests\ErrorRelics.Tests.csproj') -c Debug -r win-x64 --self-contained true `
        "-p:RuntimeFrameworkVersion=$version" "-p:Sts2Path=$ResolvedGamePath" `
        -p:DeployModOnBuild=false -p:UpdateDependencyManifest=false -o $output -v:q
    if ($LASTEXITCODE -ne 0) { throw 'Test driver publish failed.' }

    Write-Host 'Running console assertions first...' -ForegroundColor Yellow
    & (Join-Path $output 'ErrorRelics.Tests.exe') (Join-Path $ResolvedGamePath 'data_sts2_windows_x86_64')
    if ($LASTEXITCODE -ne 0) { throw 'Console assertions failed. Multiplayer launch cancelled.' }

    $testMod = Join-Path $runtime 'mods\ErrorRelics.Tests'
    New-Item $testMod -ItemType Directory -Force | Out-Null
    Copy-Item (Join-Path $output 'ErrorRelics.Tests.dll') -Destination $testMod -Force
    [IO.File]::WriteAllText(
        (Join-Path $testMod 'ErrorRelics.Tests.json'),
        '{"id":"ErrorRelics.Tests","name":"ERROR multiplayer test driver","author":"Local testing","version":"0.1.0","min_game_version":"0.111.0","has_dll":true,"has_pck":false,"affects_gameplay":true,"dependencies":[{"id":"ErrorRelics","min_version":"0.91.0"}]}'+"`n",
        [Text.UTF8Encoding]::new($false)
    )

    $session = New-Session 'auto-smoke-2p'
    $exe = Join-Path $runtime 'SlayTheSpire2.exe'
    $hostOut = Join-Path $session 'host.out.log'
    $hostErr = Join-Path $session 'host.err.log'
    $hostGodot = Join-Path $session 'host-godot.log'
    $clientOut = Join-Path $session 'client.out.log'
    $clientErr = Join-Path $session 'client.err.log'
    $clientGodot = Join-Path $session 'client-godot.log'

    $hostArgs = '--headless --audio-driver Dummy --path "'+$runtime+'" --force-steam off --log-file "'+$hostGodot+'" -fastmp host_standard --errorrelic-smoke'
    $gameHost = Start-Process -FilePath $exe -ArgumentList $hostArgs -WorkingDirectory $runtime -WindowStyle Hidden -PassThru `
        -RedirectStandardOutput $hostOut -RedirectStandardError $hostErr

    [void](Wait-ForText $hostOut "Finished mod initialization for 'BaseLib'" 25)

    $clientArgs = '--headless --audio-driver Dummy --path "'+$runtime+'" --force-steam off --log-file "'+$clientGodot+'" -fastmp join -clientId 1001 --errorrelic-smoke'
    $client = Start-Process -FilePath $exe -ArgumentList $clientArgs -WorkingDirectory $runtime -WindowStyle Hidden -PassThru `
        -RedirectStandardOutput $clientOut -RedirectStandardError $clientErr

    $deadline = [DateTime]::UtcNow.AddSeconds(240)
    $passed = $false
    while ([DateTime]::UtcNow -lt $deadline) {
        $hostPass = (Test-Path $hostOut) -and ((Get-Content $hostOut -Raw -ErrorAction SilentlyContinue) -match 'ERROR_MP_SMOKE_PASS')
        $clientPass = (Test-Path $clientOut) -and ((Get-Content $clientOut -Raw -ErrorAction SilentlyContinue) -match 'ERROR_MP_SMOKE_PASS')
        if ($hostPass -and $clientPass) { $passed = $true; break }
        if ($gameHost.HasExited -or $client.HasExited) { break }
        Start-Sleep -Milliseconds 500
    }

    Stop-TestProcesses $runtime | Out-Null
    $report = Write-LogReport $session 'ERROR Relics automatic 2P smoke test'

    $status = if ($passed) { 'PASS' } else { 'NOT PASSED / NEED LOG REVIEW' }
    [IO.File]::WriteAllText((Join-Path $session 'RESULT.txt'),
        "Result: $status`nHost smoke pass: $((Test-Path $hostOut) -and ((Get-Content $hostOut -Raw -ErrorAction SilentlyContinue) -match 'ERROR_MP_SMOKE_PASS'))`nClient smoke pass: $((Test-Path $clientOut) -and ((Get-Content $clientOut -Raw -ErrorAction SilentlyContinue) -match 'ERROR_MP_SMOKE_PASS'))`n",
        [Text.UTF8Encoding]::new($false))

    Write-Host ''
    Write-Host "AUTO SMOKE RESULT: $status" -ForegroundColor ($(if($passed){'Green'}else{'Red'}))
    Write-Host "Logs: $session"
    Write-Host "Summary: $report"
}

function Run-Gift2P([string]$ResolvedGamePath, [string]$ResolvedBaseLibPath) {
    Write-Section 'ERROR Relics - interactive Gift Relic 2P test'
    $runtime = Ensure-Runtime $ResolvedGamePath $ResolvedBaseLibPath
    Stop-TestProcesses $runtime | Out-Null
    Build-And-DeployER $ResolvedGamePath $runtime

    # The test-only DLL is deliberately disabled here. This mode is the real UI path.
    Remove-Item (Join-Path $runtime 'mods\ErrorRelics.Tests\ErrorRelics.Tests.dll') -Force -ErrorAction SilentlyContinue
    Remove-Item (Join-Path $runtime 'mods\ErrorRelics.Tests\ErrorRelics.Tests.pdb') -Force -ErrorAction SilentlyContinue

    $session = New-Session 'gift-2p'
    $exe = Join-Path $runtime 'SlayTheSpire2.exe'
    $hostOut = Join-Path $session 'host.out.log'
    $hostErr = Join-Path $session 'host.err.log'
    $hostGodot = Join-Path $session 'host-godot.log'
    $clientOut = Join-Path $session 'client.out.log'
    $clientErr = Join-Path $session 'client.err.log'
    $clientGodot = Join-Path $session 'client-godot.log'

    $hostArgs = '--windowed --path "'+$runtime+'" --force-steam off --log-file "'+$hostGodot+'" -fastmp host_standard'
    $gameHost = Start-Process -FilePath $exe -ArgumentList $hostArgs -WorkingDirectory $runtime -PassThru `
        -RedirectStandardOutput $hostOut -RedirectStandardError $hostErr

    [void](Wait-ForText $hostOut "Finished mod initialization for 'BaseLib'" 25)

    $clientArgs = '--windowed --path "'+$runtime+'" --force-steam off --log-file "'+$clientGodot+'" -fastmp join -clientId 1001'
    $client = Start-Process -FilePath $exe -ArgumentList $clientArgs -WorkingDirectory $runtime -PassThru `
        -RedirectStandardOutput $clientOut -RedirectStandardError $clientErr

    $info = @"
ERROR Relics Gift Relic 2P session

Runtime:
$runtime

Host PID:
$($gameHost.Id)

Client PID:
$($client.Id)

This session uses the isolated user dir configured by prepare-smoke.ps1:
ErrorRelics-MultiplayerTests

Minimum test path:
1. Start/ready the co-op run.
2. Reach a campfire on either peer.
3. Verify "Gift Relic" appears on both peers at the same rest-site option position.
4. Host gifts a normal relic to Client; confirm it disappears from Host and appears on Client on BOTH windows.
5. Client gifts a relic back to Host; confirm both windows agree.
6. Gift an ERROR relic and verify its H/E identity and description survive transfer.
7. Verify ERROR PROOF cannot be selected for gifting.
8. Cancel from the relic picker; then open Gift again and cancel target selection / leave the campfire.
9. If anything looks wrong, stop the test and send the entire session folder back for analysis.

Important new-feature risks:
- sender applies locally before broadcast
- receiver reconstructs from SerializableRelic
- recipient AfterObtained fires
- source inventory index must identify the same relic on every peer
- duplicate execution / inventory divergence must not occur
"@
    [IO.File]::WriteAllText((Join-Path $session 'TEST-CHECKLIST.txt'), $info, [Text.UTF8Encoding]::new($false))

    Write-Host ''
    Write-Host 'Two isolated STS2 windows have been launched.' -ForegroundColor Green
    Write-Host "Host PID:   $($gameHost.Id)"
    Write-Host "Client PID: $($client.Id)"
    Write-Host "Session logs: $session"
    Write-Host ''
    Write-Host 'Minimum Gift test:' -ForegroundColor Cyan
    Write-Host '  1) Host -> Client normal relic'
    Write-Host '  2) Client -> Host normal relic'
    Write-Host '  3) ERROR relic transfer keeps H/E'
    Write-Host '  4) ERROR PROOF is not giftable'
    Write-Host '  5) Cancel picker / cancel target / leave campfire'
    Write-Host ''
    Write-Host 'When finished, return to this launcher and choose Check Logs, or run Stop.'
}

function Run-CheckLogs {
    if (!(Test-Path $ResultsRoot)) {
        Write-Host 'No multiplayer-test-results folder exists yet.' -ForegroundColor Yellow
        return
    }
    $session = Get-ChildItem $ResultsRoot -Directory | Sort-Object LastWriteTime -Descending | Select-Object -First 1
    if (!$session) {
        Write-Host 'No test session found.' -ForegroundColor Yellow
        return
    }
    $report = Write-LogReport $session.FullName "ERROR Relics log scan"
    Write-Section 'Latest log scan'
    Get-Content $report
    Write-Host "Report: $report"
}

function Run-Stop([string]$ResolvedGamePath) {
    $runtime = Join-Path ([IO.Path]::GetFullPath($ResolvedGamePath)) '.errorrelic-multiplayer-tests'
    if (!(Test-Path (Join-Path $runtime '.errorrelic-test-runtime'))) {
        Write-Host 'No owned isolated test runtime found.' -ForegroundColor Yellow
        return
    }
    $count = Stop-TestProcesses $runtime
    Write-Host "Stopped $count isolated STS2 test process(es)." -ForegroundColor Green
}

function Show-Menu {
    while ($true) {
        Write-Section 'ERROR Relics Multiplayer Test Launcher'
        Write-Host '1. AutoSmoke  - fully automatic existing 2P multiplayer smoke test'
        Write-Host '2. Gift2P     - visible Host + Client for the new campfire Gift Relic feature'
        Write-Host '3. CheckLogs  - scan the newest session for suspicious errors'
        Write-Host '4. Stop       - stop only the isolated test STS2 instances'
        Write-Host '5. Exit'
        Write-Host ''
        $choice = Read-Host 'Choose 1-5'
        switch ($choice) {
            '1' {
                $envInfo = Resolve-Environment
                Run-AutoSmoke $envInfo.GamePath $envInfo.BaseLibPath
                Read-Host 'Press Enter to return to menu' | Out-Null
            }
            '2' {
                $envInfo = Resolve-Environment
                Run-Gift2P $envInfo.GamePath $envInfo.BaseLibPath
                Read-Host 'Press Enter to return to menu' | Out-Null
            }
            '3' {
                Run-CheckLogs
                Read-Host 'Press Enter to return to menu' | Out-Null
            }
            '4' {
                $envInfo = Resolve-Environment
                Run-Stop $envInfo.GamePath
                Read-Host 'Press Enter to return to menu' | Out-Null
            }
            '5' { return }
            default { Write-Host 'Invalid choice.' -ForegroundColor Yellow }
        }
    }
}

try {
    if ($Mode -eq 'Menu') {
        Show-Menu
    } elseif ($Mode -eq 'CheckLogs') {
        Run-CheckLogs
    } else {
        $envInfo = Resolve-Environment
        switch ($Mode) {
            'AutoSmoke' { Run-AutoSmoke $envInfo.GamePath $envInfo.BaseLibPath }
            'Gift2P'    { Run-Gift2P $envInfo.GamePath $envInfo.BaseLibPath }
            'Stop'      { Run-Stop $envInfo.GamePath }
        }
    }
} catch {
    Write-Host ''
    Write-Host 'TEST LAUNCHER FAILED' -ForegroundColor Red
    Write-Host $_.Exception.Message -ForegroundColor Red
    Write-Host ''
    Write-Host 'If you send me this console error plus the newest multiplayer-test-results folder, I can continue from there.'
    exit 1
}
