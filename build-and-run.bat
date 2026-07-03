@echo off
setlocal EnableExtensions

rem Builds the solution and starts the ASP.NET Core web application.
rem The script can be launched from any working directory because it switches
rem to the repository root first.

set "ROOT_DIR=%~dp0"
set "SOLUTION_FILE=%ROOT_DIR%StargateGalacticCommand.sln"
set "WEB_PROJECT=%ROOT_DIR%StargateGalacticCommand.Web\StargateGalacticCommand.Web.csproj"

pushd "%ROOT_DIR%" >nul 2>&1
if errorlevel 1 (
    echo [ERROR] Could not switch to repository directory: "%ROOT_DIR%"
    exit /b 1
)

where dotnet >nul 2>&1
if errorlevel 1 (
    echo [ERROR] The .NET SDK was not found in PATH.
    echo         Install .NET SDK 8.0 or newer and run this script again.
    popd >nul 2>&1
    exit /b 1
)

if not exist "%SOLUTION_FILE%" (
    echo [ERROR] Solution file not found: "%SOLUTION_FILE%"
    popd >nul 2>&1
    exit /b 1
)

if not exist "%WEB_PROJECT%" (
    echo [ERROR] Web project not found: "%WEB_PROJECT%"
    popd >nul 2>&1
    exit /b 1
)

echo Restoring NuGet packages...
dotnet restore "%SOLUTION_FILE%"
if errorlevel 1 (
    echo [ERROR] dotnet restore failed.
    popd >nul 2>&1
    exit /b 1
)

echo Building solution...
dotnet build "%SOLUTION_FILE%" --no-restore
if errorlevel 1 (
    echo [ERROR] dotnet build failed.
    popd >nul 2>&1
    exit /b 1
)

echo Starting Stargate Galactic Command...
echo Press Ctrl+C to stop the application.
dotnet run --project "%WEB_PROJECT%" --no-build
set "RUN_EXIT_CODE=%ERRORLEVEL%"

popd >nul 2>&1
exit /b %RUN_EXIT_CODE%
