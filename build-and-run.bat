@echo off
setlocal EnableExtensions

rem Builds the solution and starts the ASP.NET Core web application.
rem The script can be launched from any working directory because it switches
rem to the repository root first. When opened by double-click, the window stays
rem open after errors so the diagnostic message remains visible.
rem If the .NET SDK is missing, the script attempts a safe per-user install of
rem the latest .NET 8 SDK before continuing.

set "ROOT_DIR=%~dp0"
set "SOLUTION_FILE=%ROOT_DIR%StargateGalacticCommand.sln"
set "WEB_PROJECT=%ROOT_DIR%StargateGalacticCommand.Web\StargateGalacticCommand.Web.csproj"
set "APP_URL=http://localhost:5000"
set "DOTNET_MIN_MAJOR=8"
set "DOTNET_INSTALL_DIR=%LocalAppData%\Microsoft\dotnet"
set "DOTNET_INSTALL_SCRIPT=%TEMP%\dotnet-install-%RANDOM%%RANDOM%.ps1"
set "RUN_EXIT_CODE=0"
set "PUSHD_OK=0"

pushd "%ROOT_DIR%" >nul 2>&1
if errorlevel 1 (
    echo [ERROR] Could not switch to repository directory: "%ROOT_DIR%"
    set "RUN_EXIT_CODE=1"
    goto finish
)
set "PUSHD_OK=1"

call :ensure_dotnet_sdk
if errorlevel 1 (
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

goto finish

:ensure_dotnet_sdk
call :is_dotnet_sdk_ready
if not errorlevel 1 exit /b 0

echo [INFO] .NET SDK %DOTNET_MIN_MAJOR%.0 or newer was not found in PATH.
echo [INFO] Attempting to install .NET SDK %DOTNET_MIN_MAJOR% for the current user.
echo.

where winget >nul 2>&1
if not errorlevel 1 (
    echo Installing .NET SDK via winget...
    winget install --id Microsoft.DotNet.SDK.8 --exact --source winget --accept-package-agreements --accept-source-agreements
    if not errorlevel 1 (
        call :refresh_dotnet_path
        call :is_dotnet_sdk_ready
        if not errorlevel 1 exit /b 0
        echo [WARN] winget reported success, but dotnet is still unavailable in this session.
    ) else (
        echo [WARN] winget install failed. Falling back to the official dotnet-install script.
    )
) else (
    echo [INFO] winget was not found. Falling back to the official dotnet-install script.
)

where powershell >nul 2>&1
if errorlevel 1 (
    echo [ERROR] PowerShell was not found. Cannot download the .NET SDK installer.
    echo         Install .NET SDK %DOTNET_MIN_MAJOR%.0 or newer manually: https://dotnet.microsoft.com/download
    exit /b 1
)

echo Downloading official dotnet-install.ps1...
powershell -NoProfile -ExecutionPolicy Bypass -Command "[Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12; Invoke-WebRequest -Uri 'https://dot.net/v1/dotnet-install.ps1' -OutFile '%DOTNET_INSTALL_SCRIPT%'"
if errorlevel 1 (
    echo [ERROR] Could not download the .NET SDK installer.
    echo         Check your internet connection or install .NET SDK %DOTNET_MIN_MAJOR%.0 or newer manually.
    exit /b 1
)

echo Installing .NET SDK %DOTNET_MIN_MAJOR% into "%DOTNET_INSTALL_DIR%"...
powershell -NoProfile -ExecutionPolicy Bypass -File "%DOTNET_INSTALL_SCRIPT%" -Channel %DOTNET_MIN_MAJOR%.0 -InstallDir "%DOTNET_INSTALL_DIR%" -NoPath
set "INSTALL_RESULT=%ERRORLEVEL%"
del "%DOTNET_INSTALL_SCRIPT%" >nul 2>&1
if not "%INSTALL_RESULT%"=="0" (
    echo [ERROR] .NET SDK installation failed with exit code %INSTALL_RESULT%.
    echo         Install .NET SDK %DOTNET_MIN_MAJOR%.0 or newer manually: https://dotnet.microsoft.com/download
    exit /b 1
)

call :refresh_dotnet_path
call :is_dotnet_sdk_ready
if errorlevel 1 (
    echo [ERROR] .NET SDK was installed, but dotnet could not be started from this session.
    echo         Close this window, open a new terminal, and run build-and-run.bat again.
    exit /b 1
)

echo [INFO] .NET SDK is ready.
exit /b 0

:is_dotnet_sdk_ready
set "DOTNET_SDK_MAJOR="
set "DOTNET_SDK_MAJOR_NUM="
where dotnet >nul 2>&1
if errorlevel 1 exit /b 1

for /f "tokens=1 delims=." %%M in ('dotnet --version 2^>nul') do set "DOTNET_SDK_MAJOR=%%M"
if not defined DOTNET_SDK_MAJOR exit /b 1

set /a DOTNET_SDK_MAJOR_NUM=%DOTNET_SDK_MAJOR% 2>nul
if errorlevel 1 exit /b 1
if not defined DOTNET_SDK_MAJOR_NUM exit /b 1
if %DOTNET_SDK_MAJOR_NUM% LSS %DOTNET_MIN_MAJOR% exit /b 1
exit /b 0

:refresh_dotnet_path
if exist "%DOTNET_INSTALL_DIR%\dotnet.exe" set "PATH=%DOTNET_INSTALL_DIR%;%PATH%"
if exist "%ProgramFiles%\dotnet\dotnet.exe" set "PATH=%ProgramFiles%\dotnet;%PATH%"
exit /b 0

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
