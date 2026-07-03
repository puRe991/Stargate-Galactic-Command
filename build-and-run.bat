@echo off
setlocal EnableExtensions

rem Builds the solution and starts the ASP.NET Core web application.
rem The script can be launched from any working directory because it switches
rem to the repository root first. It keeps the console open on errors so
rem double-click users can read the actual failure message.

set "ROOT_DIR=%~dp0"
set "SOLUTION_FILE=%ROOT_DIR%StargateGalacticCommand.sln"
set "WEB_PROJECT=%ROOT_DIR%StargateGalacticCommand.Web\StargateGalacticCommand.Web.csproj"
set "APP_URL=http://localhost:5000"
set "DID_PUSHD=0"

pushd "%ROOT_DIR%" >nul 2>&1
if errorlevel 1 (
    echo [ERROR] Could not switch to repository directory: "%ROOT_DIR%"
    set "EXIT_CODE=1"
    goto fail
)
set "DID_PUSHD=1"

where dotnet >nul 2>&1
if errorlevel 1 (
    echo [ERROR] The .NET SDK was not found in PATH.
    echo         Install .NET SDK 8.0 or newer and run this script again.
    set "EXIT_CODE=1"
    goto fail
)

if not exist "%SOLUTION_FILE%" (
    echo [ERROR] Solution file not found: "%SOLUTION_FILE%"
    set "EXIT_CODE=1"
    goto fail
)

if not exist "%WEB_PROJECT%" (
    echo [ERROR] Web project not found: "%WEB_PROJECT%"
    set "EXIT_CODE=1"
    goto fail
)

echo Restoring NuGet packages...
dotnet restore "%SOLUTION_FILE%"
if errorlevel 1 (
    echo [ERROR] dotnet restore failed.
    set "EXIT_CODE=1"
    goto fail
)

echo Building solution...
dotnet build "%SOLUTION_FILE%" --no-restore
if errorlevel 1 (
    echo [ERROR] dotnet build failed.
    set "EXIT_CODE=1"
    goto fail
)

echo Starting Stargate Galactic Command at %APP_URL% ...
echo Leave this window open while you use the application.
echo Press Ctrl+C in this window to stop the application.
echo.

rem Bind to a predictable local URL and open the browser before dotnet run
rem takes over this console. If the port is already in use, ASP.NET Core will
rem print the concrete error and the pause below keeps it visible.
set "ASPNETCORE_URLS=%APP_URL%"
start "" "%APP_URL%"

dotnet run --project "%WEB_PROJECT%" --no-build
set "EXIT_CODE=%ERRORLEVEL%"

if not "%EXIT_CODE%"=="0" (
    echo.
    echo [ERROR] The application stopped with exit code %EXIT_CODE%.
    goto fail
)

echo.
echo Stargate Galactic Command was stopped.
goto finish

:fail
if not defined EXIT_CODE set "EXIT_CODE=1"
if "%DID_PUSHD%"=="1" popd >nul 2>&1
pause
exit /b %EXIT_CODE%

:finish
if not defined EXIT_CODE set "EXIT_CODE=0"
if "%DID_PUSHD%"=="1" popd >nul 2>&1
pause
exit /b %EXIT_CODE%
