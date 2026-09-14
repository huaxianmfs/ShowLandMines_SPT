@echo off
setlocal
cd /d "%~dp0"

set SPT_DIR=..\SPT-5.0.0-47242-BE
set PLUGIN_DIR=%SPT_DIR%\BepInEx\plugins

dotnet build ShowLandMines.csproj -c Release
if errorlevel 1 (
    echo BUILD FAILED.
    pause
    exit /b 1
)

echo BUILD SUCCEEDED.
echo Copying ShowLandMines.dll to plugins...
copy /Y "bin\Release\net472\ShowLandMines.dll" "%PLUGIN_DIR%\ShowLandMines.dll"

pause