@echo off
setlocal EnableExtensions

rem -----------------------------------------------------------------------------
rem Stargate Galactic Command - Windows build and run helper
rem -----------------------------------------------------------------------------
rem Restores NuGet packages, builds the solution and starts the ASP.NET Core web
rem app on a predictable local URL. The console stays open on failures and after
rem the app stops, so double-click users can read diagnostics.
rem
rem Usage:
rem   build-and-run.bat [--url http://localhost:5000] [--no-browser] [--no-pause]
rem -----------------------------------------------------------------------------

set "ROOT_DIR=%~dp0"
set "SOLUTION_FILE=%ROOT_DIR%StargateGalacticCommand.sln"
set "WEB_PROJECT=%ROOT_DIR%StargateGalacticCommand.Web\StargateGalacticCommand.Web.csproj"
set "APP_URL=http://localhost:5000"
set "OPEN_BROWSER=1"
set "KEEP_OPEN=1"
set "DID_PUSHD=0"
set "EXIT_CODE=0"

:parse_args
if "%~1"=="" goto args_done
if /I "%~1"=="--help" goto usage
if /I "%~1"=="/help" goto usage
if /I "%~1"=="-h" goto usage
if /I "%~1"=="--no-browser" (
    set "OPEN_BROWSER=0"
    shift
    goto parse_args
)
if /I "%~1"=="--no-pause" (
    set "KEEP_OPEN=0"
    shift
    goto parse_args
)
if /I "%~1"=="--url" (
    if "%~2"=="" (
        echo [ERROR] Missing value for --url.
        set "EXIT_CODE=2"
        goto fail
    )
    set "APP_URL=%~2"
    shift
    shift
    goto parse_args
)
echo [ERROR] Unknown argument: %~1
set "EXIT_CODE=2"
goto fail

:args_done
call :validate_url "%APP_URL%"
if errorlevel 1 goto fail

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

rem Bind to a predictable local URL. The browser is opened by a background
rem PowerShell process after a short delay so Kestrel has time to start first.
set "ASPNETCORE_URLS=%APP_URL%"
if "%OPEN_BROWSER%"=="1" (
    start "Open Stargate Galactic Command" /min powershell -NoProfile -ExecutionPolicy Bypass -Command "Start-Sleep -Seconds 3; Start-Process '%APP_URL%'"
)

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

:validate_url
set "URL_TO_VALIDATE=%~1"
if not defined URL_TO_VALIDATE (
    echo [ERROR] APP_URL is empty.
    set "EXIT_CODE=2"
    exit /b 1
)
echo %URL_TO_VALIDATE%| findstr /I /B /C:"http://" /C:"https://" >nul
if errorlevel 1 (
    echo [ERROR] Invalid URL: %URL_TO_VALIDATE%
    echo         Use an absolute URL such as http://localhost:5000.
    set "EXIT_CODE=2"
    exit /b 1
)
exit /b 0

:usage
echo Usage: build-and-run.bat [--url http://localhost:5000] [--no-browser] [--no-pause]
echo.
echo Options:
echo   --url URL       Bind ASP.NET Core to the given URL. Default: http://localhost:5000
echo   --no-browser    Do not open the default browser automatically.
echo   --no-pause      Do not pause before closing the console window.
echo   --help          Show this help.
set "EXIT_CODE=0"
goto finish

:fail
if not defined EXIT_CODE set "EXIT_CODE=1"
if "%DID_PUSHD%"=="1" popd >nul 2>&1
if not "%KEEP_OPEN%"=="0" pause
exit /b %EXIT_CODE%

:finish
if not defined EXIT_CODE set "EXIT_CODE=0"
if "%DID_PUSHD%"=="1" popd >nul 2>&1
if not "%KEEP_OPEN%"=="0" pause
exit /b %EXIT_CODE%
