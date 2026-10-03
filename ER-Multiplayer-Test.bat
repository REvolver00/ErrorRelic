@echo off
setlocal
cd /d "%~dp0"

where pwsh >nul 2>nul
if not errorlevel 1 (
    echo Using PowerShell 7...
    pwsh -NoLogo -NoProfile -ExecutionPolicy Bypass -File "%~dp0ER-Multiplayer-Test.ps1"
    set ERR=%ERRORLEVEL%
    if not "%ERR%"=="0" pause
    exit /b %ERR%
)

where powershell >nul 2>nul
if not errorlevel 1 (
    echo PowerShell 7 not found. Falling back to Windows PowerShell 5.1...
    powershell -NoLogo -NoProfile -ExecutionPolicy Bypass -File "%~dp0ER-Multiplayer-Test.ps1"
    set ERR=%ERRORLEVEL%
    if not "%ERR%"=="0" pause
    exit /b %ERR%
)

echo.
echo ERROR: No PowerShell executable was found.
echo.
pause
exit /b 1
