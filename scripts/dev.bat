@echo off
REM Viadex Device Portal - Simplified Daily Development Script (Windows)
REM Quick startup for development after initial setup

setlocal enabledelayedexpansion

echo 🚀 Viadex Device Portal - Development Startup
echo ==============================================
echo.

REM Quick checks
echo ⚡ Quick environment check...

REM Check if node_modules exists
if not exist "..\device-portal-web\node_modules" (
    echo ⚠️  Frontend dependencies not found
    echo 💡 Run 'setup.bat' first to set up your environment
    exit /b 1
)

REM Check if SQL Server is running
docker ps --filter name=mssql --filter status=running --quiet | findstr /r "." >nul
if %errorlevel% neq 0 (
    echo ⚠️  SQL Server container not running
    echo 🚀 Starting SQL Server...
    
    REM Remove existing container if it exists but is stopped
    docker rm mssql >nul 2>&1
    
    REM Start SQL Server
    docker run -d -e "ACCEPT_EULA=Y" -e "MSSQL_SA_PASSWORD=Pa55w0rd" -p 1433:1433 -v mssql-data:/var/opt/mssql --name mssql mcr.microsoft.com/mssql/server
    if %errorlevel% neq 0 (
        echo ❌ Failed to start SQL Server container
        exit /b 1
    )
    
    echo ⏳ Waiting for SQL Server to be ready...
    timeout /t 10 /nobreak >nul
    
    REM Wait for SQL Server to be ready
    set max_attempts=15
    set attempt=0
    :wait_loop
    docker exec mssql /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P Pa55w0rd -Q "SELECT 1" -C >nul 2>&1
    if %errorlevel% equ 0 goto sql_ready
    
    set /a attempt+=1
    if !attempt! geq !max_attempts! (
        echo ❌ SQL Server failed to start within 30 seconds
        echo 💡 Try running 'scripts/setup.bat' for a complete environment reset
        exit /b 1
    )
    echo ⏳ Attempt !attempt!/!max_attempts! - SQL Server not ready yet...
    timeout /t 2 /nobreak >nul
    goto wait_loop
    :sql_ready
)

echo ✅ Environment ready
echo.

REM Start services
echo 🚀 Starting development services...
echo 📝 Services will be available at:
echo    Frontend: http://localhost:5173
echo    API:      https://localhost:7027
echo.
echo 💡 Press Ctrl+C to stop all services
echo.

REM Start API in background
cd ..\DevicePortal.Api
echo 🔧 Starting API server...
start /B dotnet watch --launch-profile https
cd ..

REM Give API a moment to start
timeout /t 3 /nobreak >nul

REM Start frontend (this will be the foreground process)
cd ..\device-portal-web
echo 🎨 Starting frontend dev server...
npm run dev
cd ..