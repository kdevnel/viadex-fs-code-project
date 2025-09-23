#!/bin/bash
# Viadex Device Portal - Simplified Daily Development Script
# Quick startup for development after initial setup

set -e

# Detect if we're running from project root or scripts directory
SCRIPT_DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" &> /dev/null && pwd )"

if [ -f "package.json" ]; then
    # Running from project root
    PROJECT_ROOT="$(pwd)"
    DEVICE_WEB_DIR="$PROJECT_ROOT/device-portal-web"
    API_DIR="$PROJECT_ROOT/DevicePortal.Api"
    echo -e "${BLUE}📁 Detected execution from project root${NC}"
elif [ -f "../package.json" ]; then
    # Running from scripts directory
    PROJECT_ROOT="$(dirname "$SCRIPT_DIR")"
    DEVICE_WEB_DIR="$PROJECT_ROOT/device-portal-web"
    API_DIR="$PROJECT_ROOT/DevicePortal.Api"
    echo -e "${BLUE}📁 Detected execution from scripts directory${NC}"
else
    echo -e "${RED}❌ Cannot detect project structure. Please run from project root or scripts directory.${NC}"
    echo -e "${YELLOW}💡 Current directory: $(pwd)${NC}"
    echo -e "${YELLOW}💡 Contents: $(ls -la)${NC}"
    exit 1
fi

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[0;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

echo -e "${BLUE}🚀 Viadex Device Portal - Development Startup${NC}"
echo "=============================================="
echo

# Quick checks
echo -e "${BLUE}⚡ Quick environment check...${NC}"

# Check if node_modules exists
if [ ! -d "$DEVICE_WEB_DIR/node_modules" ]; then
    echo -e "${YELLOW}⚠️  Frontend dependencies not found${NC}"
    echo -e "${YELLOW}💡 Run 'npm run setup' first to set up your environment${NC}"
    exit 1
fi

# Check for existing processes on our ports
if lsof -i :7027 >/dev/null 2>&1; then
    echo -e "${RED}❌ Port 7027 is already in use${NC}"
    echo -e "${YELLOW}💡 Another process is using the API port. Please stop it first:${NC}"
    echo -e "   ${YELLOW}lsof -i :7027${NC} to see what's using it"
    echo -e "   ${YELLOW}kill <PID>${NC} to stop the process"
    exit 1
fi

if lsof -i :5173 >/dev/null 2>&1; then
    echo -e "${RED}❌ Port 5173 is already in use${NC}"
    echo -e "${YELLOW}💡 Another process is using the frontend port. Please stop it first:${NC}"
    echo -e "   ${YELLOW}lsof -i :5173${NC} to see what's using it"
    echo -e "   ${YELLOW}kill <PID>${NC} to stop the process"
    exit 1
fi

# Check if SQL Server is running
if ! docker ps --filter name=mssql --filter status=running --quiet | grep -q .; then
    echo -e "${YELLOW}⚠️  SQL Server container not running${NC}"
    echo -e "${BLUE}🚀 Starting SQL Server...${NC}"
    
    # Remove existing container if it exists but is stopped
    docker rm mssql >/dev/null 2>&1 || true
    
    docker run -d \
        -e "ACCEPT_EULA=Y" \
        -e "MSSQL_SA_PASSWORD=Pa55w0rd" \
        -p 1433:1433 \
        -v mssql-data:/var/opt/mssql \
        --name mssql \
        mcr.microsoft.com/mssql/server
    
    echo -e "${BLUE}⏳ Waiting for SQL Server to be ready...${NC}"
    sleep 10
    
    # Wait for SQL Server to be ready
    max_attempts=15
    attempt=0
    while ! docker exec mssql /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P Pa55w0rd -Q "SELECT 1" -C >/dev/null 2>&1; do
        ((attempt++))
        if [ $attempt -ge $max_attempts ]; then
            echo -e "${RED}❌ SQL Server failed to start within 30 seconds${NC}"
            echo -e "${YELLOW}💡 Try running 'npm run setup' for a complete environment reset${NC}"
            exit 1
        fi
        echo -e "${BLUE}⏳ Attempt $attempt/$max_attempts - SQL Server not ready yet...${NC}"
        sleep 2
    done
fi

echo -e "${GREEN}✅ Environment ready${NC}"
echo

# Start services
echo -e "${BLUE}🚀 Starting development services...${NC}"
echo -e "${YELLOW}📝 Services will be available at:${NC}"
echo -e "   Frontend: ${GREEN}http://localhost:5173${NC}"
echo -e "   API:      ${GREEN}https://localhost:7027${NC}"
echo
echo -e "${YELLOW}💡 Press Ctrl+C to stop all services${NC}"
echo

# Function to kill background processes on exit
cleanup() {
    echo
    echo -e "${YELLOW}🛑 Shutting down services...${NC}"
    
    # Kill background jobs (our own processes only)
    jobs -p | xargs -r kill 2>/dev/null || true
    
    echo -e "${GREEN}✅ Services stopped${NC}"
    exit 0
}

# Set up signal handlers
trap cleanup SIGINT SIGTERM

# Start API in background
echo -e "${BLUE}🔧 Starting API server...${NC}"
if [ -d "$API_DIR" ]; then
    cd "$API_DIR"
    dotnet watch --launch-profile https &
    API_PID=$!
else
    echo -e "${RED}❌ API directory not found: $API_DIR${NC}"
    echo -e "${YELLOW}💡 Current directory: $(pwd)${NC}"
    echo -e "${YELLOW}💡 Looking for: $API_DIR${NC}"
    exit 1
fi

# Give API a moment to start
sleep 3

# Start frontend in background
echo -e "${BLUE}🎨 Starting frontend dev server...${NC}"
if [ -d "$DEVICE_WEB_DIR" ]; then
    cd "$DEVICE_WEB_DIR"
    npm run dev &
    FRONTEND_PID=$!
else
    echo -e "${RED}❌ Frontend directory not found: $DEVICE_WEB_DIR${NC}"
    echo -e "${YELLOW}💡 Current directory: $(pwd)${NC}"
    echo -e "${YELLOW}💡 Looking for: $DEVICE_WEB_DIR${NC}"
    exit 1
fi

# Wait for any process to exit
wait