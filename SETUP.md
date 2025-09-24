# Setting up the project locally

## Prerequisites

### Required Software
- **Node.js** 20.19+ or 22.12+ ([nodejs.org](https://nodejs.org))
- **.NET 9 SDK** ([dotnet.microsoft.com](https://dotnet.microsoft.com))
- **Docker Desktop** ([docker.com](https://docker.com))

### Platform-Specific Notes

#### macOS Setup
```bash
# Using Homebrew (recommended)
brew install node
brew install dotnet
# Download Docker Desktop from docker.com
```

#### Windows Setup (**untested as I'm on Mac, sorry!**)
Please note, the Windows instructions are automated by AI since I don't have a Windows system to confirm on currently.

```powershell
# Using Chocolatey (recommended)
choco install nodejs
choco install dotnet-9.0-sdk
# Download Docker Desktop from docker.com

# Or download installers directly from official websites
```

## Quick Start (Automated Setup)

The fastest way to get running - works on both Windows (**untested**) and macOS. If preferred, you can run things more manually by going to the Manual Setup section.

### 1. Clone and Navigate
```bash
git clone https://github.com/kdevnel/viadex-fs-code-project.git
cd viadex-fs-code-project
```

### 2. One-Command Setup
```bash
# macOS/Linux
npm run setup
npm run dev

# Windows
npm run setup:win
npm run dev:win
```

**That's it!** The setup script will:
- ✅ Install all dependencies
- ✅ Configure environment variables
- ✅ Start SQL Server container
- ✅ Apply database migrations
- ✅ Seed sample data
- ✅ Start development servers

**Access the application:**
- 🎨 **Frontend**: http://localhost:5173
- 🔧 **API**: https://localhost:7027

## Manual Setup (Step-by-Step)

If you prefer manual control or need to troubleshoot:

### 1. Start SQL Server Database

#### Option A: Standard SQL Server (Recommended)
```bash
docker run -d \
  -e "ACCEPT_EULA=Y" \
  -e "MSSQL_SA_PASSWORD=Pa55w0rd" \
  -p 1433:1433 \
  -v mssql-data:/var/opt/mssql \
  --name mssql \
  mcr.microsoft.com/mssql/server
```

**Database Connection Details:**
- Server: `localhost,1433`
- Database: `DevicePortal`
- Username: `sa`
- Password: `Pa55w0rd`

### 2. Setup Backend API

```bash
# Navigate to API project
cd DevicePortal.Api

# Trust HTTPS certificate (first time only)
dotnet dev-certs https --trust

# Install EF Core tools
dotnet tool install --global dotnet-ef

# Create database and apply migrations
dotnet ef database update

# Seed with sample data
dotnet run -- seed

# Start API with hot reload
dotnet watch --launch-profile https
# I had to run it this way - dotnet watch _may_ work for you.
```

### 3. Setup Frontend

**Open a new terminal:**
```bash
# Navigate to frontend project
cd device-portal-web

# Install dependencies
npm install

# Create environment file
echo "VITE_API_BASE_URL=https://localhost:7027" > .env.development

# Start development server
npm run dev
```

## API Endpoints

### Device Management
- `GET /api/devices?status=Active&page=1&pageSize=20` - List with filtering
- `GET /api/devices/{id}` - Get by ID
- `POST /api/devices` - Create device
- `GET /api/devices/status-distribution` - Chart data

### Shipment Tracking
- `GET /api/shipments/track/{trackingNumber}` - Track by number
- `GET /api/shipments?status=InTransit&page=1&pageSize=20` - List with filtering
- `GET /api/shipments/status-distribution` - Chart data
- `POST /api/shipments` - Create shipment
- `PATCH /api/shipments/{id}/status` - Update status

### Quote Generation (Ready for Implementation)
- `POST /api/quotes/calculate` - Calculate quote
- `GET /api/quotes/{id}` - Get quote details

## Troubleshooting

### Common Issues (All Platforms)

#### SQL Server Connection
```bash
# Check container status
docker ps | grep mssql

# View logs
docker logs mssql

# Restart container
docker restart mssql
```

#### HTTPS Certificate Issues
```bash
# Reset certificates
dotnet dev-certs https --clean
dotnet dev-certs https --trust
```

#### Port Conflicts
- **SQL Server**: 1433
- **API**: 7027  
- **Frontend**: 5173

If ports are in use, update configuration files:
- `DevicePortal.Api/Properties/launchSettings.json` (API port)
- `device-portal-web/vite.config.ts` (Frontend port)
- Docker command (SQL port)

## Project Structure

```
viadex-device-portal/
├── DevicePortal.Api/           # ASP.NET Core API
│   ├── Controllers/            # HTTP endpoints
│   ├── Services/              # Business logic
│   ├── Models/                # Domain entities
│   ├── DTOs/                  # API contracts
│   └── Data/                  # EF Core context
├── device-portal-web/         # Vue 3 Frontend
│   ├── src/views/             # Page components
│   ├── src/stores/            # Pinia state management
│   ├── src/services/          # API client
│   └── src/types/             # TypeScript interfaces
└── scripts/                   # Setup and development scripts
```

## Testing the Application

**Frontend**: Visit http://localhost:5173