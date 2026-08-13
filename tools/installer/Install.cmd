@echo off
rem Double-click launcher for install.ps1.
rem -ExecutionPolicy Bypass is what lets this run straight out of a downloaded zip,
rem where the extracted .ps1 still carries the mark-of-the-web.
setlocal
powershell.exe -NoProfile -NoLogo -ExecutionPolicy Bypass -File "%~dp0install.ps1" %*
if errorlevel 1 (
    echo.
    echo The installer could not start. Windows PowerShell 5.1 or later is required.
    pause
)
endlocal
