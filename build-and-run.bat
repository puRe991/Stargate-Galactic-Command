@echo off
setlocal EnableExtensions

rem Builds the solution and starts the ASP.NET Core web application.
rem The script can be launched from any working directory because it switches
rem to the repository root first. When opened by double-click, the window stays
rem open after errors so the diagnostic message remains visible.

set "ROOT_DIR=%~dp0"
set "SOLUTION_FILE=%ROOT_DIR%StargateGalacticCommand.sln"
set "WEB_PROJECT=%ROOT_DIR%StargateGalacticCommand.Web\StargateGalacticCommand.Web.csproj"
set "APP_URL=http://localhost:5000"
set "RUN_EXIT_CODE=0"
set "PUSHD_OK=0"

pushd "%ROOT_DIR%" >nul 2>&1
if errorlevel 1 (
    echo [ERROR] Could not switch to repository directory: "%ROOT_DIR%"
    set "RUN_EXIT_CODE=1"
    goto finish
)
set "PUSHD_OK=1"

where dotnet >nul 2>&1
if errorlevel 1 (
    echo [ERROR] The .NET SDK was not found in PATH.
    echo         Install .NET SDK 8.0 or newer and run this script again.
    set "RUN_EXIT_CODE=1"
    goto finish
)

if not exist "%SOLUTION_FILE%" (
    echo [ERROR] Solution file not found: "%SOLUTION_FILE%"
    set "RUN_EXIT_CODE=1"
    goto finish
)

if not exist "%WEB_PROJECT%" (
    echo [ERROR] Web project not found: "%WEB_PROJECT%"
    set "RUN_EXIT_CODE=1"
    goto finish
)

echo Restoring NuGet packages...
dotnet restore "%SOLUTION_FILE%"
if errorlevel 1 (
    echo [ERROR] dotnet restore failed.
    set "RUN_EXIT_CODE=1"
    goto finish
)

echo Building solution...
dotnet build "%SOLUTION_FILE%" --no-restore
if errorlevel 1 (
    echo [ERROR] dotnet build failed.
    set "RUN_EXIT_CODE=1"
    goto finish
)

echo Starting Stargate Galactic Command...
echo Opening %APP_URL% in your default browser.
echo Press Ctrl+C to stop the application.

rem Open the browser shortly after Kestrel starts. The timeout process runs in a
rem separate window so the web server can continue to stream logs in this one.
start "Stargate Galactic Command Browser Launcher" /min powershell -NoProfile -ExecutionPolicy Bypass -Command "Start-Sleep -Seconds 3; Start-Process '%APP_URL%'"
set "ASPNETCORE_URLS=%APP_URL%"
dotnet run --project "%WEB_PROJECT%" --no-build --urls "%APP_URL%"
set "RUN_EXIT_CODE=%ERRORLEVEL%"

:finish
if "%PUSHD_OK%"=="1" popd >nul 2>&1
if not "%RUN_EXIT_CODE%"=="0" (
    echo.
    echo [ERROR] build-and-run.bat stopped with exit code %RUN_EXIT_CODE%.
    echo         The window remains open so you can read the message above.
    pause
) else (
    echo.
    echo Stargate Galactic Command stopped.
    pause
)
exit /b %RUN_EXIT_CODE%
