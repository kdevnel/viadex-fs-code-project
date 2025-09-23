@echo off
REM Viadex Device Portal - Initial Setup Script (Windows)
REM Run this once to set up your development environment

setlocal enabledelayedexpansion

echo 🔧 Viadex Device Portal - Initial Setup
echo =====================================
echo.

REM Check prerequisites
echo 1️⃣  Checking prerequisites...
node --version >nul 2>&1
if %errorlevel% neq 0 (
    echo ❌ Node.js is required but not installed. Please install Node.js 18+ and try again.
    exit /b 1
)

dotnet --version >nul 2>&1
if %errorlevel% neq 0 (
    echo ❌ .NET SDK is required but not installed. Please install .NET 9 SDK and try again.
    exit /b 1
)

docker --version >nul 2>&1
if %errorlevel% neq 0 (
    echo ❌ Docker is required but not installed. Please install Docker Desktop and try again.
    exit /b 1
)

docker info >nul 2>&1
if %errorlevel% neq 0 (
    echo ❌ Docker is not running. Please start Docker Desktop and try again.
    exit /b 1
)

echo ✅ Prerequisites check passed
echo.

REM Install dependencies
echo 2️⃣  Installing dependencies...
if not exist "..\device-portal-web\node_modules" (
    cd ..\device-portal-web
    npm install
    if %errorlevel% neq 0 (
        echo ❌ Failed to install dependencies
        exit /b 1
    )
    cd ..
    echo ✅ Frontend dependencies installed
) else (
    echo ✅ Frontend dependencies already installed
)
echo.

REM Create environment file
echo 3️⃣  Creating environment configuration...
if not exist "..\device-portal-web\.env.development" (
    echo VITE_API_BASE_URL=https://localhost:7027 > ..\device-portal-web\.env.development
    echo ✅ Environment file created
) else (
    echo ✅ Environment file already exists
)
echo.

REM Trust development certificates
echo 4️⃣  Setting up HTTPS certificates...
dotnet dev-certs https --trust >nul 2>&1
echo ✅ HTTPS certificates configured
echo.

REM Start SQL Server
echo 5️⃣  Setting up SQL Server...
docker ps --filter name=mssql --filter status=running --quiet | findstr /r "." >nul
if %errorlevel% equ 0 (
    echo ✅ SQL Server is already running
) else (
    REM Remove existing container if it exists but is stopped
    docker rm mssql >nul 2>&1
    
    echo 🚀 Starting SQL Server container...
    docker run -d -e "ACCEPT_EULA=Y" -e "MSSQL_SA_PASSWORD=Pa55w0rd" -p 1433:1433 -v mssql-data:/var/opt/mssql --name mssql mcr.microsoft.com/mssql/server
    if %errorlevel% neq 0 (
        echo ❌ Failed to start SQL Server container
        exit /b 1
    )
    
    echo ⏳ Waiting for SQL Server to be ready...
    timeout /t 15 /nobreak >nul
    
    REM Wait for SQL Server to be ready
    set max_attempts=30
    set attempt=0
    :wait_loop
    docker exec mssql /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P Pa55w0rd -Q "SELECT 1" -C >nul 2>&1
    if %errorlevel% equ 0 (
        echo ✅ SQL Server is ready
        goto sql_ready
    )
    set /a attempt+=1
    echo ⏳ Attempt !attempt!/!max_attempts! - SQL Server not ready yet...
    if !attempt! geq !max_attempts! (
        echo ❌ SQL Server failed to start within 60 seconds
        echo 💡 Try running: docker stop mssql && docker rm mssql && setup.bat
        exit /b 1
    )
    timeout /t 2 /nobreak >nul
    goto wait_loop
    :sql_ready
)
echo.

REM Run database migrations and seed
echo 6️⃣  Setting up database...
cd ..\DevicePortal.Api

echo 🗃️  Applying database migrations...
dotnet ef database update
if %errorlevel% neq 0 (
    echo ❌ Failed to apply database migrations
    exit /b 1
)
echo ✅ Database migrations applied

echo 🌱 Seeding database with sample data...
dotnet run -- seed
if %errorlevel% neq 0 (
    echo ❌ Failed to seed database
    exit /b 1
)
echo ✅ Database seeded

cd ..
echo.

echo 🎉 Setup Complete!
echo ==================
echo.
echo Your development environment is now ready.
echo.
echo Next steps:
echo   • Run 'npm run dev:win' to start development servers
echo   • Open http://localhost:5173 for the frontend
echo   • Open https://localhost:7027 for the API
echo.
echo 💡 You only need to run this setup script once.
echo    For daily development, just use 'npm run dev:win'