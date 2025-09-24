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

## Localization & Context
⚠️ **IMPORTANT**: This application uses **UK context** for all sample data and examples:
- Use UK addresses (postcodes, cities like London, Manchester, Edinburgh, Birmingham, Cardiff, Brighton)
- Use British names (Thompson, Williams, Davies, Clarke, Roberts, etc.)
- Use UK formatting conventions where applicable
- Address format: "Number Street, City, Postcode" (e.g., "42 Baker Street, London, W1U 3AA")
- Use GBP (£) for currency values

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
6. **Status handling** converts numeric enums to display strings seamlessly

### Critical Status Handling Pattern
```typescript
// Backend returns numeric enum values (1, 2, 3)
// Frontend handles both numeric and string values
const statusDistribution = computed(() => {
  const distribution: Record<string, number> = {
    Active: 0, Retired: 0, UnderRepair: 0
  };
  
  devices.value.forEach(device => {
    const statusAsNumber = Number(device.status);
    const statusAsString = String(device.status);
    
    // Handle both 1/'Active', 2/'Retired', 3/'UnderRepair'
    if (statusAsNumber === 1 || statusAsString === 'Active') {
      distribution['Active']++;
    } // ... etc
  });
});
```

### Component Structure (Current Implementation)
```
device-portal-web/src/
├── views/
│   ├── DevicesView.vue            # ✅ Device management with status system
│   ├── ShipmentTrackerView.vue    # 📋 Track shipments by number
│   └── QuoteGeneratorView.vue     # 📋 Calculate leasing quotes
├── components/
│   ├── DeviceTable.vue            # ✅ Implemented in DevicesView.vue
│   ├── DeviceStatusChart.vue      # 📋 Device distribution by status
│   ├── ShipmentStatusChart.vue    # 📋 Shipments by status/ETA
│   └── QuoteCalculator.vue        # 📋 Quote form with validation
├── stores/
│   ├── useDevices.ts              # ✅ Device management with status distribution
│   ├── useShipments.ts            # 📋 Shipment tracking state
│   └── useQuotes.ts               # 📋 Quote calculation state
├── services/
│   ├── api.ts                     # ✅ Centralized native fetch API client
│   ├── deviceApi.ts               # ✅ Device CRUD operations
│   ├── shipmentApi.ts             # 📋 Shipment tracking API
│   └── quoteApi.ts                # 📋 Quote calculation API
└── types/
    ├── device.ts                  # ✅ Device interfaces with status
    ├── shipment.ts                # 📋 Shipment tracking types
    └── quote.ts                   # 📋 Quote calculation types
```

## Domain Models (Current Implementation)

### Device Model (✅ Complete)
```csharp
// Models/Device.cs - Fully implemented with status
public class Device
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Model { get; set; } = "";
    public decimal MonthlyPrice { get; set; }
    public DateTime PurchaseDate { get; set; }
    public DeviceStatus Status { get; set; } = DeviceStatus.Active; // ✅ Implemented
}

public enum DeviceStatus
{
    Active = 1,
    Retired = 2,
    UnderRepair = 3
}
```

### Shipment Model (✅ Fully Implemented)
```csharp
// Models/Shipment.cs - Complete tracking functionality
public class Shipment
{
    public int Id { get; set; }
    public string TrackingNumber { get; set; } = ""; // Unique index in DB
    public string CustomerName { get; set; } = "";
    public ShipmentStatus Status { get; set; } = ShipmentStatus.Processing;
    public DateTime EstimatedDelivery { get; set; }
    public DateTime? ActualDelivery { get; set; }
    public string Destination { get; set; } = "";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public enum ShipmentStatus
{
    Processing = 1,
    InTransit = 2,
    Delivered = 3,
    Delayed = 4
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

## API Endpoints Status

### Device Endpoints (✅ Fully Implemented)
```csharp
// Controllers/DevicesController.cs
GET  /api/devices?status=Active&page=1&pageSize=20  // ✅ Implemented
GET  /api/devices/{id}                              // ✅ Implemented
POST /api/devices                                   // ✅ Implemented
GET  /api/devices/status-distribution               // ✅ Ready for implementation
```

### Shipment Endpoints (✅ Fully Implemented)
```csharp
// Controllers/ShipmentsController.cs
GET  /api/shipments/track/{trackingNumber}           // ✅ Core tracking feature
GET  /api/shipments?status=InTransit&page=1&pageSize=20  // ✅ Filtering + pagination
GET  /api/shipments/{id}                            // ✅ Get by ID
GET  /api/shipments/status-distribution             // ✅ Chart data ready
POST /api/shipments                                 // ✅ Create with validation
PATCH /api/shipments/{id}/status                    // ✅ Update status with business rules
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
1. **Device Status Distribution** - ✅ Data Available: Pie/donut chart showing Active, Retired, Under Repair
2. **Shipment Status Distribution** - ✅ Data Available: Bar chart showing Processing, InTransit, Delivered, Delayed
3. **Optional**: Monthly pricing trends, quote distribution by support tier

### Chart Implementation Pattern (Ready for Integration)
```typescript
// Device status data is already computed and reactive
const deviceStatusData = computed(() => ({
  labels: ['Active', 'Retired', 'Under Repair'],
  datasets: [{
    data: [
      deviceStore.statusDistribution.Active,
      deviceStore.statusDistribution.Retired,
      deviceStore.statusDistribution.UnderRepair
    ],
    backgroundColor: ['#10b981', '#6b7280', '#f59e0b']
  }]
}))

// Shipment status data is available from API
const shipmentStatusData = computed(() => ({
  labels: ['Processing', 'In Transit', 'Delivered', 'Delayed'],
  datasets: [{
    data: [
      shipmentStore.statusDistribution.Processing,
      shipmentStore.statusDistribution.InTransit,
      shipmentStore.statusDistribution.Delivered,
      shipmentStore.statusDistribution.Delayed
    ],
    backgroundColor: ['#3b82f6', '#f59e0b', '#10b981', '#ef4444']
  }]
}))
```

- HTTPS port: `7027` (in `launchSettings.json`)

## Essential Files Reference

- `Program.cs` - DI container, middleware pipeline, database commands
- `Services/DeviceService.cs` - Core business logic with result patterns  
- `Services/ShipmentService.cs` - ✅ Shipment business logic and validation
- `Controllers/DevicesController.cs` - HTTP handling and validation
- `Controllers/ShipmentsController.cs` - ✅ Shipment HTTP endpoints
- `DTOs/DeviceDtos.cs` - API contracts with validation rules
- `DTOs/ShipmentDtos.cs` - ✅ Shipment API contracts
- `Extensions/ShipmentMappingExtensions.cs` - ✅ DTO/Entity mapping
- `Scripts/SeedDatabase.cs` - Database seeding/clearing utilities (enhanced for shipments)
- `Data/AppDbContext.cs` - EF Core configuration (includes Shipment DbSet)

## Development Workflow

1. **Database**: Start Docker container → run migrations → seed data
2. **API**: `dotnet watch` for hot reload
3. **Testing**: Use Swagger UI (uncomment in Program.cs), database scripts, or curl commands
4. **New features**: Follow Controller → Service → DTO → Extension mapping pattern

### Enhanced Database Commands
```bash
# Seed all data (devices + shipments)
dotnet run -- seed

# Seed specific entities
dotnet run -- seed-devices
dotnet run -- seed-shipments

# Clear specific entities  
dotnet run -- clear-devices
dotnet run -- clear-shipments

# Full reseed
dotnet run -- reseed
```

### Testing Shipment Endpoints
```bash
# Track shipment
curl -k "https://localhost:7027/api/shipments/track/TRK001234567"

# Get status distribution for charts
curl -k "https://localhost:7027/api/shipments/status-distribution"

# Filter by status
curl -k "https://localhost:7027/api/shipments?status=2"
```

When adding new entities, maintain the established patterns: create service interface, implement business logic in service, create DTOs with validation, add mapping extensions, and implement thin controllers.