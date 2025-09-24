#!/bin/bash

# Viadex Device Portal - Initial Setup Script
# Run this once to set up your development environment

set -e  # Exit on any error

# Detect if we're running from project root or scripts directory
if [ -f "package.json" ]; then
    # Running from project root
    PROJECT_ROOT="."
    DEVICE_WEB_DIR="device-portal-web"
    API_DIR="DevicePortal.Api"
elif [ -f "../package.json" ]; then
    # Running from scripts directory
    PROJECT_ROOT=".."
    DEVICE_WEB_DIR="../device-portal-web"
    API_DIR="../DevicePortal.Api"
else
    echo "❌ Cannot detect project structure. Please run from project root or scripts directory."
    exit 1
fi

echo "🔧 Viadex Device Portal - Initial Setup"
echo "====================================="
echo ""

# Check if Node.js is installed
echo "1️⃣  Checking prerequisites..."
if ! command -v node &> /dev/null; then
    echo "❌ Node.js is required but not installed. Please install Node.js 18+ and try again."
    exit 1
fi

if ! command -v dotnet &> /dev/null; then
    echo "❌ .NET SDK is required but not installed. Please install .NET 9 SDK and try again."
    exit 1
fi

# Check if dotnet ef tool is installed
if ! dotnet ef --version &> /dev/null; then
    echo "🔧 Installing Entity Framework tools..."
    dotnet tool install --global dotnet-ef
    if [ $? -ne 0 ]; then
        echo "❌ Failed to install dotnet-ef tools"
        exit 1
    fi
    echo "✅ Entity Framework tools installed"
fi

if ! command -v docker &> /dev/null; then
    echo "❌ Docker is required but not installed. Please install Docker Desktop and try again."
    exit 1
fi

if ! docker info &> /dev/null; then
    echo "❌ Docker is not running. Please start Docker Desktop and try again."
    exit 1
fi

echo "✅ Prerequisites check passed"
echo ""

# Install dependencies
echo "2️⃣  Installing dependencies..."
if [ ! -d "$DEVICE_WEB_DIR/node_modules" ]; then
    cd "$DEVICE_WEB_DIR"
    npm install
    if [ $? -ne 0 ]; then
        echo -e "${RED}❌ Failed to install dependencies${NC}"
        exit 1
    fi
    cd "$PROJECT_ROOT"
    echo -e "${GREEN}✅ Frontend dependencies installed${NC}"
else
    echo -e "${GREEN}✅ Frontend dependencies already installed${NC}"
fi
echo ""

# Create environment file
echo "3️⃣  Creating environment configuration..."
if [ ! -f "$DEVICE_WEB_DIR/.env.development" ]; then
    echo "VITE_API_BASE_URL=https://localhost:7027" > "$DEVICE_WEB_DIR/.env.development"
    echo -e "${GREEN}✅ Environment file created${NC}"
else
    echo -e "${GREEN}✅ Environment file already exists${NC}"
fi
echo ""

# Trust development certificates
echo "4️⃣  Setting up HTTPS certificates..."
dotnet dev-certs https --trust 2>/dev/null || echo "ℹ️  Certificate trust may require manual confirmation"
echo "✅ HTTPS certificates configured"
echo ""

# Start SQL Server
echo "5️⃣  Setting up SQL Server..."
if docker ps --filter name=mssql --filter status=running --quiet | grep -q .; then
    echo "✅ SQL Server is already running"
else
    # Remove existing container if it exists but is stopped
    docker rm mssql 2>/dev/null || true
    
    echo "🚀 Starting SQL Server container..."
    docker run -d \
        -e "ACCEPT_EULA=Y" \
        -e "MSSQL_SA_PASSWORD=Pa55w0rd" \
        -p 1433:1433 \
        -v mssql-data:/var/opt/mssql \
        --name mssql \
        mcr.microsoft.com/mssql/server
    
    echo "⏳ Waiting for SQL Server to be ready..."
    sleep 15
    
    # Wait for SQL Server to be ready
    max_attempts=30
    attempt=0
    while [ $attempt -lt $max_attempts ]; do
        if docker exec mssql /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P Pa55w0rd -Q "SELECT 1" -C &>/dev/null; then
            echo "✅ SQL Server is ready"
            break
        fi
        attempt=$((attempt + 1))
        echo "⏳ Attempt $attempt/$max_attempts - SQL Server not ready yet..."
        sleep 2
    done
    
    if [ $attempt -eq $max_attempts ]; then
        echo "❌ SQL Server failed to start within 60 seconds"
        echo "💡 Try running: docker stop mssql && docker rm mssql && ./setup.sh"
        exit 1
    fi
fi
echo ""

# Run database migrations and seed
echo "6️⃣  Setting up database..."
cd "$API_DIR"

echo "🗃️  Applying database migrations..."
if dotnet ef database update; then
    echo "✅ Database migrations applied"
else
    echo "❌ Failed to apply database migrations"
    exit 1
fi

echo "🌱 Seeding database with sample data..."
if dotnet run -- seed; then
    echo "✅ Database seeded"
else
    echo "❌ Failed to seed database"
    exit 1
fi

cd "$PROJECT_ROOT"
echo ""

echo "🎉 Setup Complete!"
echo "================"
echo ""
echo "Your development environment is now ready."
echo ""
echo "Next steps:"
echo "  • Run 'npm run dev' to start development servers"
echo "  • Open http://localhost:5173 for the frontend"
echo "  • Open https://localhost:7027 for the API"
echo ""
echo "💡 You only need to run this setup script once."
echo "   For daily development, just use 'npm run dev'"