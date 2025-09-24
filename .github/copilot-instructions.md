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

### Vue 3 + TypeScript Architecture
The frontend follows established integration patterns with the API:

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
│   ├── Devices.vue                # ✅ Device management with status system
│   ├── Shipments.vue              # ✅ Shipment tracking with UK context
│   └── QuoteGeneratorView.vue     # 📋 Calculate leasing quotes
├── components/
│   ├── DeviceStatusChart.vue      # 📋 Device distribution by status
│   ├── ShipmentStatusChart.vue    # 📋 Shipments by status/ETA
│   └── QuoteCalculator.vue        # 📋 Quote form with validation
├── stores/
│   ├── useDevices.ts              # ✅ Device management with status distribution
│   ├── useShipments.ts            # ✅ Shipment tracking state with persistence
│   └── useQuotes.ts               # 📋 Quote calculation state
├── services/
│   ├── api.ts                     # ✅ Centralized native fetch API client
│   ├── deviceApi.ts               # ✅ Device CRUD operations
│   ├── shipmentApi.ts             # ✅ Shipment tracking API operations
│   └── quoteApi.ts                # 📋 Quote calculation API
└── types/
    ├── device.ts                  # ✅ Device interfaces with status
    ├── shipment.ts                # ✅ Shipment tracking types with enums
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

### Shipment Model (✅ Complete with Frontend)
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
    public string Destination { get; set; } = ""; // UK addresses
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

**Frontend Implementation**: ✅ Complete Vue interface with status filtering, overview cards, and UK context

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

**Frontend Integration**: ✅ Complete Vue interface with unified component patterns

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

## Unified Frontend View Patterns

### ✅ Established Vue Component Architecture
The frontend follows **unified patterns** across all views for consistency and maintainability. All new views should follow these established patterns based on DevicesView and ShipmentsView implementations.

### 1. Component Structure Pattern
```vue
<template>
  <!-- Status Overview Cards -->
  <section class="overview">
    <div 
      v-for="card in statusCards" 
      :key="card.label"
      :class="['overview-card', { active: filters.status === card.status }]"
      @click="setStatusFilter(card.status)"
    >
      <h3>{{ card.label }}</h3>
      <p class="count" :style="{ color: card.color }">{{ card.value }}</p>
    </div>
  </section>

  <!-- List Header with Controls -->
  <div class="list-header">
    <h2>{{ entityName }} Management</h2>
    <div class="list-controls">
      <select 
        v-model="filters.status" 
        @change="updateStatusFilter" 
        class="status-filter"
      >
        <option :value="undefined">All Statuses</option>
        <option v-for="status in statusOptions" :key="status.value" :value="status.value">
          {{ status.label }}
        </option>
      </select>
      <button @click="clearFilters" class="clear-filters">Clear Filters</button>
    </div>
  </div>

  <!-- CSS Grid Table Layout -->
  <div class="entity-table">
    <div class="table-header">
      <div>Column 1</div>
      <div>Column 2</div>
      <!-- Additional columns -->
    </div>
    <div v-for="item in store.items" :key="item.id" class="table-row">
      <div class="col-name">
        <strong>{{ item.name }}</strong>
      </div>
      <!-- Additional data columns -->
      <div class="col-actions">
        <button @click="handleView(item)">View</button>
        <button @click="handleEdit(item)">Edit</button>
        <button @click="handleDelete(item)">Delete</button>
      </div>
    </div>
  </div>

  <!-- Unified Pagination -->
  <div class="pagination">
    <button 
      @click="store.goToPage(store.filters.page - 1)"
      :disabled="store.filters.page <= 1"
      class="page-button"
    >
      Previous
    </button>
    <span class="page-info">
      Page {{ store.filters.page }} of {{ store.totalPages }}
      ({{ store.totalItems }} total)
    </span>
    <button 
      @click="store.goToPage(store.filters.page + 1)"
      :disabled="store.filters.page >= store.totalPages"
      class="page-button"
    >
      Next
    </button>
  </div>
</template>
```

### 2. Script Composition Pattern
```typescript
<script setup lang="ts">
import { reactive, computed, onMounted } from 'vue';
import { useEntityStore } from '@/stores/useEntity';
import type { Entity, EntityStatus } from '@/types/entity';

// Store initialization
const store = useEntityStore();

// Local reactive state
const filters = reactive({
  status: undefined as number | undefined
});

// Status configuration (centralized constants)
const ENTITY_STATUS_COLORS = {
  [EntityStatus.Status1]: '#10b981', // Green
  [EntityStatus.Status2]: '#6b7280', // Gray  
  [EntityStatus.Status3]: '#f59e0b'  // Orange
} as const;

// Computed status cards for overview
const statusCards = computed(() => [
  {
    label: 'Status 1',
    value: store.statusDistribution.status1,
    color: ENTITY_STATUS_COLORS[EntityStatus.Status1],
    status: EntityStatus.Status1
  },
  // Additional status cards...
]);

// Unified methods pattern
const setStatusFilter = async (status?: number) => {
  filters.status = status;
  await store.setStatusFilter(status);
};

const updateStatusFilter = async () => {
  await store.setStatusFilter(filters.status);
};

const clearFilters = async () => {
  filters.status = undefined;
  await store.clearFilters();
};

// CRUD operation handlers
const handleView = (item: Entity) => {
  store.selectEntity(item);
  // Navigate or show modal
};

const handleEdit = (item: Entity) => {
  // Navigate to edit form
};

const handleDelete = async (item: Entity) => {
  if (confirm(`Delete "${item.name}"?`)) {
    const result = await store.deleteEntity(item.id);
    if (!result.success) {
      alert('Failed to delete: ' + result.error);
    }
  }
};

// Utility functions
const getStatusClass = (status?: number | string) => {
  // Convert numeric to string and map to CSS classes
  // Return: 'status-badge status-{name}'
};

// Initialize on mount
onMounted(async () => {
  await store.fetchEntities();
});
</script>
```

### 3. Pinia Store Pattern
```typescript
// stores/useEntity.ts
export const useEntityStore = defineStore('entity', {
  state: () => ({
    items: [] as Entity[],
    filters: {
      page: 1,
      pageSize: 20,
      status: undefined as number | undefined
    },
    loading: false,
    error: null as string | null,
    selectedEntity: null as Entity | null
  }),

  getters: {
    // Status distribution for overview cards
    statusDistribution(): Record<string, number> {
      return this.items.reduce((acc, item) => {
        const status = String(item.status);
        acc[status] = (acc[status] || 0) + 1;
        return acc;
      }, {} as Record<string, number>);
    },

    totalPages(): number {
      return Math.ceil(this.totalItems / this.filters.pageSize);
    },

    totalItems(): number {
      // Filter items based on current filters
      return this.getFilteredItems().length;
    }
  },

  actions: {
    // Unified filtering methods
    async setStatusFilter(status?: number) {
      this.filters.status = status;
      this.filters.page = 1; // Reset to first page
      await this.fetchEntities();
    },

    async clearFilters() {
      this.filters = { page: 1, pageSize: 20, status: undefined };
      await this.fetchEntities();
    },

    async goToPage(page: number) {
      if (page >= 1 && page <= this.totalPages) {
        this.filters.page = page;
        await this.fetchEntities();
      }
    },

    // CRUD operations with result pattern
    async fetchEntities() {
      this.loading = true;
      this.error = null;
      try {
        const result = await entityApi.getEntities(this.filters);
        if (result.isSuccess) {
          this.items = result.items || [];
        } else {
          this.error = result.errorMessage || 'Failed to fetch';
        }
      } catch (error) {
        this.error = 'Network error occurred';
      } finally {
        this.loading = false;
      }
    }
  },

  persist: {
    storage: localStorage,
    paths: ['filters'] // Persist user preferences
  }
});
```

### 4. CSS Grid Table Layout Pattern
```scss
.entity-table {
  background: white;
  border-radius: 8px;
  overflow: hidden;
  box-shadow: 0 1px 3px rgba(0, 0, 0, 0.1);
}

.table-header {
  display: grid;
  grid-template-columns: 2fr 1.5fr 1fr 1.5fr 1.2fr 2fr; /* Adjust columns */
  gap: 1rem;
  padding: 1rem;
  background: #f8fafc;
  font-weight: 600;
  color: #374151;
  border-bottom: 1px solid #e5e7eb;
}

.table-row {
  display: grid;
  grid-template-columns: 2fr 1.5fr 1fr 1.5fr 1.2fr 2fr; /* Match header */
  gap: 1rem;
  padding: 1rem;
  border-bottom: 1px solid #f3f4f6;
  transition: background-color 0.2s ease;
}

.table-row:hover {
  background: #f8fafc;
}
```

### 5. Status Handling Pattern
```typescript
// types/entity.ts - Numeric enums for API compatibility
export enum EntityStatus {
  Status1 = 1,
  Status2 = 2,
  Status3 = 3
}

// Centralized color constants
export const ENTITY_STATUS_COLORS = {
  [EntityStatus.Status1]: '#10b981', // Success green
  [EntityStatus.Status2]: '#6b7280', // Neutral gray
  [EntityStatus.Status3]: '#f59e0b'  // Warning orange
} as const;

// Status display mapping
export const ENTITY_STATUS_LABELS = {
  [EntityStatus.Status1]: 'Active',
  [EntityStatus.Status2]: 'Inactive', 
  [EntityStatus.Status3]: 'Pending'
} as const;
```

### 6. Required View Features Checklist
Every view implementing this pattern must include:

✅ **Status Overview Cards** - Interactive cards showing distribution with filtering
✅ **Unified Filtering** - Status dropdown + clear filters button
✅ **CSS Grid Table** - Responsive grid layout instead of HTML tables  
✅ **Consistent Pagination** - Previous/Next with page info
✅ **CRUD Actions** - View/Edit/Delete with confirmation
✅ **Error Handling** - Loading states and error display
✅ **Status Badges** - Consistent styling with centralized colors
✅ **Responsive Design** - Mobile-friendly layouts
✅ **State Persistence** - Filter preferences saved to localStorage

### 7. File Organization Requirements
```
src/
├── views/
│   └── EntityView.vue              # Main view component
├── stores/
│   └── useEntity.ts               # Pinia store with unified methods
├── services/
│   └── entityApi.ts               # API client functions
├── types/
│   └── entity.ts                  # TypeScript interfaces + enums
└── components/ (optional)
    └── EntitySpecificComponent.vue # View-specific components
```

### 8. Integration with Backend Result Pattern
```typescript
// Handle API responses consistently
const result = await entityApi.getEntities(filters);
if (result.isSuccess) {
  this.items = result.items || [];
  this.totalItems = result.total || 0;
} else {
  this.error = result.errorMessage || 'Operation failed';
}
```

### 9. UK Context Requirements
All sample data and UI text must use UK context:
- **Addresses**: UK postcodes (e.g., "W1U 3AA"), British cities
- **Names**: British surnames (Thompson, Williams, Davies)  
- **Currency**: GBP (£) formatting
- **Dates**: DD/MM/YYYY or British locale formatting

### 10. Development Guidelines
- **New Views**: Copy DevicesView.vue structure and adapt for entity type
- **Store Creation**: Follow useDevicesStore.ts pattern exactly
- **API Integration**: Use centralized api.ts client with native fetch
- **Status Systems**: Always use numeric enums with color constants
- **Error Handling**: Never throw exceptions, use result patterns
- **Styling**: Use centralized CSS classes from `src/assets/styles/` - avoid duplicating styles

### 11. Centralized CSS Classes Reference
All views should use these centralized classes instead of duplicating styles:

**Layout & Structure:**
- `.page-view` - Main page container
- `.page-header` - Page title section  
- `.section-spacing` - Consistent section margins
- `.stats-grid` - Overview cards grid layout
- `.list-header` - Header with title and controls
- `.list-controls` - Filter and action controls

**Grid Tables (Modern approach):**
- `.grid-table` - Table container with shadow and borders
- `.grid-table-header` - Table header with background
- `.grid-table-row` - Table row with hover effects
- `.grid-6-cols`, `.grid-5-cols`, `.grid-4-cols` - Column templates

**Table Cells:**
- `.cell-name` - Primary content (names, titles)
- `.cell-secondary` - Secondary content (descriptions, dates)
- `.cell-price` - Price formatting with green color
- `.cell-actions` - Action button containers

**Status & Badges:**
- `.status-badge` - Base status badge styling
- `.status-active`, `.status-retired`, `.status-repair` - Device statuses
- `.status-processing`, `.status-in-transit`, `.status-delivered`, `.status-delayed` - Shipment statuses

**Controls & Actions:**
- `.btn`, `.btn-primary`, `.btn-secondary` - Button variants
- `.status-filter` - Status dropdown styling
- `.clear-filters` - Clear filters button
- `.pagination`, `.page-button`, `.page-info` - Pagination controls

**Cards & Stats:**
- `.stat-card` - Interactive status overview cards
- `.stat-value`, `.stat-label` - Card content styling

**Forms:**
- `.form-group`, `.form-label`, `.form-input` - Form components
- `.filter-input`, `.filter-select` - Filter-specific inputs

**Responsive:** All components include mobile-first responsive design automatically.