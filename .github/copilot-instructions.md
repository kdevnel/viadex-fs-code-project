# Viadex Device Portal - AI Coding Guidelines

## Project Overview

This is a **full-stack device leasing and customer portal application** combining:
1. **Device Leasing/Analytics** - Device management, pricing, lifecycle data
2. **Customer Portal** - Shipment tracking and quote generation

### Core Features (Per Original Brief)
1. **Devices View** - List devices with status information (Active, Retired, Under Repair)
2. **Shipment Tracker** - Track shipments by tracking number with status details
3. **Quote Generator** - Calculate leasing quotes (device + duration + support tier)
4. **Data Visualization** - Charts from live API data (device status distribution, shipment status)

## Architecture Overview

This is a **service-oriented ASP.NET Core API** with enhanced architecture patterns. The design emphasizes separation of concerns through distinct layers:

- **Controllers** (`Controllers/`) - HTTP concerns only, delegate to services
- **Services** (`Services/`, `Services/Interfaces/`) - Business logic and validation
- **DTOs** (`DTOs/`) - API contracts with validation attributes
- **Models** (`Models/`) - Domain entities for EF Core
- **Extensions** (`Extensions/`) - Mapping between DTOs and entities
- **Data** (`Data/`) - EF Core context and database configuration

## Key Patterns

### Result Pattern for Error Handling
Never throw exceptions for business logic failures. Use result types:
```csharp
// In Services/Interfaces/IDeviceService.cs
public static DevicePagedResult Success(int total, List<Device> items) => 
    new() { IsSuccess = true, Total = total, Items = items };
public static DevicePagedResult Failure(string errorMessage) => 
    new() { IsSuccess = false, ErrorMessage = errorMessage };
```

### DTO/Entity Mapping via Extensions
Use extension methods for example in `Extensions/DeviceMappingExtensions.cs`:
```csharp
// Convert DTO to Entity
var device = createDto.ToEntity();
// Convert Entity to Response DTO  
var responseDto = device.ToDto();
```

### Service Layer Business Logic
Controllers are thin - business logic lives in services:
```csharp
// Controllers validate and delegate
var result = await _deviceService.GetDevicesAsync(page, pageSize);
return result.IsSuccess ? Ok(result) : BadRequest(result.ErrorMessage);
```

### Validation Strategy
Multi-layer validation:
1. **DTO level**: Data annotations (`[Required]`, `[Range]`, `[StringLength]`)
2. **Service level**: Business rules (uniqueness, complex validation)
3. **Database level**: Constraints (`decimal(7,2)` for MonthlyPrice)

## Database Operations

### Development Database Setup
```bash
# Apple Silicon (M1/M2)
docker run --platform linux/arm64 -e "ACCEPT_EULA=Y" -e "MSSQL_SA_PASSWORD=Pa55w0rd" -p 1433:1433 -v mssql-data:/var/opt/mssql --name sqledge -d mcr.microsoft.com/azure-sql-edge

# Apply migrations
dotnet ef database update
```

### Custom Database Commands (Program.cs integration)
The application supports database management via command line:
```bash
dotnet run -- seed     # Add sample devices (only if empty)
dotnet run -- clear    # Remove ALL devices
dotnet run -- reseed   # Clear + seed (perfect for testing)
```

Implementation in `Scripts/SeedDatabase.cs` with methods like `SeedDevicesAsync()`, `ClearDevicesAsync()`.

## Code Style & Conventions

### Spacing and Formatting
Follow the established pattern with spaces around parentheses:
```csharp
// Correct style (matches existing code)
builder.Services.AddDbContext<AppDbContext>( o =>
    o.UseSqlServer( builder.Configuration.GetConnectionString( "DevicePortal" ) ) );

// Method calls and conditions
if ( await context.Devices.AnyAsync() )
```

### Configuration Structure
- Connection string name: `"DevicePortal"` (not "DefaultConnection")
- CORS configured for `http://localhost:5173` (planned Vue frontend)
- HTTPS port: `7027` (in `launchSettings.json`)

## Frontend Integration Patterns

### Planned Vue 3 + TypeScript Architecture
The frontend will follow these integration patterns with the API:

### API Client Structure
```typescript
// services/api.ts - Centralized API client
interface DeviceCreateRequest {
  name: string;
  model: string;
  monthlyPrice: number;
}

interface DeviceResponse {
  id: number;
  name: string;
  model: string;
  monthlyPrice: number;
  purchaseDate: string;
}

interface PagedResponse<T> {
  total: number;
  items: T[];
}
```

### State Management with Pinia
```typescript
// stores/useDevices.ts - Device store with persistence
export const useDevicesStore = defineStore('devices', {
  state: () => ({
    devices: [] as DeviceResponse[],
    filters: {
      page: 1,
      pageSize: 20,
      searchTerm: ''
    },
    loading: false,
    error: null as string | null
  }),
  
  persist: {
    storage: localStorage,
    paths: ['filters'] // Persist user preferences
  }
})
```

### Error Handling Strategy
Map API result patterns to frontend states:
```typescript
// Handle API result pattern responses
if (!result.isSuccess) {
  store.error = result.errorMessage;
  // Show toast notification or error banner
}
```

### Chart Integration Patterns
```typescript
// Chart data derived from API responses
const chartData = computed(() => ({
  labels: devices.value.map(d => d.model),
  datasets: [{
    label: 'Monthly Price by Model',
    data: devices.value.map(d => d.monthlyPrice)
  }]
}))
```

### Environment Configuration
```bash
# .env.development
VITE_API_BASE_URL=https://localhost:7027
```

### Frontend-Backend Data Flow
1. **Vue components** call Pinia store actions
2. **Store actions** use API client with centralized error handling
3. **API responses** map directly to TypeScript interfaces
4. **Reactive state** updates trigger component re-renders
5. **User preferences** (filters, page) persist via localStorage

### Component Structure (Planned)
```
device-portal-web/src/
├── views/
│   ├── DevicesView.vue            # Device management with status charts
│   ├── ShipmentTrackerView.vue    # Track shipments by number
│   └── QuoteGeneratorView.vue     # Calculate leasing quotes
├── components/
│   ├── DeviceTable.vue            # Paginated table with status filtering
│   ├── DeviceStatusChart.vue      # Device distribution by status
│   ├── ShipmentStatusChart.vue    # Shipments by status/ETA
│   └── QuoteCalculator.vue        # Quote form with validation
├── stores/
│   ├── useDevices.ts              # Device management with status
│   ├── useShipments.ts            # Shipment tracking state
│   └── useQuotes.ts               # Quote calculation state
├── services/
│   ├── api.ts                     # Centralized API client
│   ├── deviceApi.ts               # Device CRUD operations
│   ├── shipmentApi.ts             # Shipment tracking API
│   └── quoteApi.ts                # Quote calculation API
└── types/
    ├── device.ts                  # Device interfaces with status
    ├── shipment.ts                # Shipment tracking types
    └── quote.ts                   # Quote calculation types
```

## Domain Models (Full Implementation)

### Device Model (Enhanced)
```csharp
// Models/Device.cs - Include status information
public class Device
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Model { get; set; } = "";
    public decimal MonthlyPrice { get; set; }
    public DateTime PurchaseDate { get; set; }
    public DeviceStatus Status { get; set; } // Active, Retired, Under Repair
}

public enum DeviceStatus
{
    Active = 1,
    Retired = 2,
    UnderRepair = 3
}
```

### Shipment Model (Missing)
```csharp
// Models/Shipment.cs - For tracking functionality
public class Shipment
{
    public int Id { get; set; }
    public string TrackingNumber { get; set; } = "";
    public string CustomerName { get; set; } = "";
    public ShipmentStatus Status { get; set; }
    public DateTime EstimatedDelivery { get; set; }
    public DateTime? ActualDelivery { get; set; }
    public string Destination { get; set; } = "";
}

public enum ShipmentStatus
{
    InTransit = 1,
    Delivered = 2,
    Delayed = 3,
    Processing = 4
}
```

### Quote Model (Missing)
```csharp
// Models/Quote.cs - For quote generation
public class Quote
{
    public int Id { get; set; }
    public int DeviceId { get; set; }
    public Device Device { get; set; } = null!;
    public int DurationMonths { get; set; }
    public SupportTier SupportTier { get; set; }
    public decimal MonthlyRate { get; set; }
    public decimal TotalCost { get; set; }
    public DateTime CreatedAt { get; set; }
}

public enum SupportTier
{
    Basic = 1,      // +0% of monthly price
    Standard = 2,   // +20% of monthly price
    Premium = 3     // +50% of monthly price
}
```

## Required API Endpoints (Per Brief)

### Device Endpoints (Enhanced)
```csharp
// Controllers/DevicesController.cs
GET  /api/devices?status=Active&page=1&pageSize=20
GET  /api/devices/{id}
POST /api/devices
GET  /api/devices/status-distribution  // For charts
```

### Shipment Endpoints (Missing)
```csharp
// Controllers/ShipmentsController.cs
GET  /api/shipments/track/{trackingNumber}
GET  /api/shipments?status=InTransit
GET  /api/shipments/status-distribution  // For charts
POST /api/shipments  // Admin: create shipment
```

### Quote Endpoints (Missing)
```csharp
// Controllers/QuotesController.cs
POST /api/quotes/calculate  // Calculate quote from device+duration+tier
GET  /api/quotes/{id}
GET  /api/quotes  // List user's quotes
```

## Chart Requirements (Per Brief)

### Required Charts
1. **Device Status Distribution** - Pie/donut chart showing Active, Retired, Under Repair
2. **Shipment Status Distribution** - Bar chart showing In Transit, Delivered, Delayed
3. **Optional**: Monthly pricing trends, quote distribution by support tier

### Chart Implementation Pattern
```typescript
// Use Chart.js or ECharts with live API data
const deviceStatusData = computed(() => ({
  labels: ['Active', 'Retired', 'Under Repair'],
  datasets: [{
    data: [
      devices.value.filter(d => d.status === 'Active').length,
      devices.value.filter(d => d.status === 'Retired').length,
      devices.value.filter(d => d.status === 'UnderRepair').length
    ]
  }]
}))
```

- HTTPS port: `7027` (in `launchSettings.json`)

## Essential Files Reference

- `Program.cs` - DI container, middleware pipeline, database commands
- `Services/DeviceService.cs` - Core business logic with result patterns  
- `Controllers/DevicesController.cs` - HTTP handling and validation
- `DTOs/DeviceDtos.cs` - API contracts with validation rules
- `Scripts/SeedDatabase.cs` - Database seeding/clearing utilities
- `Data/AppDbContext.cs` - EF Core configuration

## Development Workflow

1. **Database**: Start Docker container → run migrations → seed data
2. **API**: `dotnet watch` for hot reload
3. **Testing**: Use Swagger UI (uncomment in Program.cs) or database scripts
4. **New features**: Follow Controller → Service → DTO → Extension mapping pattern

When adding new entities, maintain the established patterns: create service interface, implement business logic in service, create DTOs with validation, add mapping extensions, and implement thin controllers.