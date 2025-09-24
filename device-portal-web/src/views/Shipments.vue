<template>
  <div class="page-view">
    <!-- Header Section -->
    <div class="page-header">
      <h1 class="page-title">Shipment Tracker</h1>
      <p class="page-subtitle">Track shipments by number and monitor delivery status across your supply chain</p>
    </div>

    <!-- Tracking Section -->
    <div class="content-section section-spacing">
      <div class="tracking-card">
        <h2 class="section-title">Track Your Shipment</h2>
        <div class="tracking-form">
          <div class="input-group">
            <input
              v-model="trackingInput"
              type="text"
              placeholder="Enter tracking number (e.g., TRK001234567)"
              class="tracking-input"
              @keyup.enter="handleTrackShipment"
              :disabled="shipmentsStore.trackingLoading"
            />
            <button
              @click="handleTrackShipment"
              class="track-button"
              :disabled="!trackingInput.trim() || shipmentsStore.trackingLoading"
            >
              <span v-if="shipmentsStore.trackingLoading">Tracking...</span>
              <span v-else>Track</span>
            </button>
          </div>
        </div>

        <!-- Tracking Result -->
        <div v-if="shipmentsStore.trackingResult" class="tracking-result">
          <div class="result-header">
            <h3>Shipment Found</h3>
            <button @click="shipmentsStore.clearTrackingResult" class="clear-button">✕</button>
          </div>
          <div class="shipment-details">
            <div class="detail-row">
              <span class="label">Tracking Number:</span>
              <span class="value">{{ shipmentsStore.trackingResult.trackingNumber }}</span>
            </div>
            <div class="detail-row">
              <span class="label">Customer:</span>
              <span class="value">{{ shipmentsStore.trackingResult.customerName }}</span>
            </div>
            <div class="detail-row">
              <span class="label">Status:</span>
              <span class="value">
                <span :class="['status-badge', getStatusClass(shipmentsStore.trackingResult.status)]">
                  {{ shipmentsStore.trackingResult.statusName }}
                </span>
              </span>
            </div>
            <div class="detail-row">
              <span class="label">Destination:</span>
              <span class="value">{{ shipmentsStore.trackingResult.destination }}</span>
            </div>
            <div class="detail-row">
              <span class="label">Estimated Delivery:</span>
              <span class="value">{{ formatDate(shipmentsStore.trackingResult.estimatedDelivery) }}</span>
            </div>
            <div v-if="shipmentsStore.trackingResult.actualDelivery" class="detail-row">
              <span class="label">Actual Delivery:</span>
              <span class="value">{{ formatDate(shipmentsStore.trackingResult.actualDelivery) }}</span>
            </div>
            <div class="detail-row">
              <span class="label">Created:</span>
              <span class="value">{{ formatDate(shipmentsStore.trackingResult.createdAt) }}</span>
            </div>
          </div>
        </div>

        <!-- Tracking Error -->
        <div v-if="shipmentsStore.trackingError" class="error-message">
          <p>{{ shipmentsStore.trackingError }}</p>
        </div>
      </div>
    </div>

    <!-- Status Overview Section -->
    <div class="stats-section section-spacing">
      <div class="stats-grid">
          <div
            v-for="stat in statusStats"
            :key="stat.label"
            class="stat-card"
            :class="{ 'active': filters.status === stat.status }"
            @click="setStatusFilter(stat.status)"
          >
            <div class="stat-value" :style="{ color: stat.color }">{{ stat.value }}</div>
            <div class="stat-label">{{ stat.label }}</div>
          </div>
        </div>
    </div>

    <!-- Shipment Status Chart Section -->
    <div class="chart-section section-spacing">
      <ShipmentStatusChart />
    </div>

    <!-- Shipments List Section -->
    <div class="content-section">
      <div class="list-header">
        <h2 class="section-title">Shipments</h2>
        <div class="list-controls">
          <select v-model="filters.status" @change="updateStatusFilter" class="status-filter">
            <option :value="undefined">All Statuses</option>
            <option :value="1">Processing</option>
            <option :value="2">In Transit</option>
            <option :value="3">Delivered</option>
            <option :value="4">Delayed</option>
          </select>
          <button v-if="shipmentsStore.activeFiltersCount > 0" @click="clearFilters" class="clear-filters">
            Clear Filters ({{ shipmentsStore.activeFiltersCount }})
          </button>
        </div>
      </div>

      <!-- Loading State -->
      <div v-if="shipmentsStore.loading" class="loading-state">
        <div class="loading-spinner"></div>
        <p>Loading shipments...</p>
      </div>

      <!-- Error State -->
      <div v-else-if="shipmentsStore.error" class="error-state">
        <p>{{ shipmentsStore.error }}</p>
        <button @click="shipmentsStore.fetchShipments" class="retry-button">Try Again</button>
      </div>

      <!-- Empty State -->
      <div v-else-if="!shipmentsStore.hasShipments" class="empty-state">
        <div class="empty-icon">📦</div>
        <h3>No shipments found</h3>
        <p v-if="shipmentsStore.activeFiltersCount > 0">
          Try adjusting your filters to see more results.
        </p>
        <p v-else>
          Shipments will appear here once they are created.
        </p>
      </div>

      <!-- Shipments Table -->
      <div v-else class="grid-table">
        <div class="grid-table-header grid-6-cols">
          <div>Tracking Number</div>
          <div>Customer</div>
          <div>Status</div>
          <div>Destination</div>
          <div>Est. Delivery</div>
          <div>Created</div>
        </div>

        <div
          v-for="shipment in shipmentsStore.currentPageShipments"
          :key="shipment.id"
          class="grid-table-row grid-6-cols"
          @click="trackSpecificShipment(shipment.trackingNumber)"
        >
          <div class="cell-name">
            <strong>{{ shipment.trackingNumber }}</strong>
          </div>
          <div class="cell-secondary">{{ shipment.customerName }}</div>
          <div>
            <span :class="['status-badge', getStatusClass(shipment.status)]">
              {{ shipment.statusName }}
            </span>
          </div>
          <div class="cell-secondary">{{ shipment.destination }}</div>
          <div class="cell-secondary">{{ formatDate(shipment.estimatedDelivery) }}</div>
          <div class="cell-secondary">{{ formatDate(shipment.createdAt) }}</div>
        </div>
      </div>

      <!-- Pagination -->
      <div v-if="shipmentsStore.totalPages > 1" class="pagination">
        <button
          @click="goToPage(shipmentsStore.filters.page - 1)"
          :disabled="shipmentsStore.filters.page <= 1"
          class="page-button"
        >
          Previous
        </button>

        <span class="page-info">
          Page {{ shipmentsStore.filters.page }} of {{ shipmentsStore.totalPages }}
          ({{ shipmentsStore.totalShipments }} total)
        </span>

        <button
          @click="goToPage(shipmentsStore.filters.page + 1)"
          :disabled="shipmentsStore.filters.page >= shipmentsStore.totalPages"
          class="page-button"
        >
          Next
        </button>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted, reactive } from 'vue';
import { useShipmentsStore } from '@/stores/useShipments';
import { ShipmentStatus, SHIPMENT_STATUS_COLORS } from '@/types/shipment';
import ShipmentStatusChart from '@/components/ShipmentStatusChart.vue';

// Component name for Vue DevTools
defineOptions({
  name: 'ShipmentTrackerView'
});

const shipmentsStore = useShipmentsStore();

// Local state
const trackingInput = ref('');

// Reactive filters that sync with store
const filters = reactive({
  status: shipmentsStore.filters.status
});

// Status statistics for overview cards
const statusStats = computed(() => [
  {
    label: 'Processing',
    value: shipmentsStore.statusDistribution.processing,
    color: SHIPMENT_STATUS_COLORS[ShipmentStatus.Processing],
    status: ShipmentStatus.Processing
  },
  {
    label: 'In Transit',
    value: shipmentsStore.statusDistribution.inTransit,
    color: SHIPMENT_STATUS_COLORS[ShipmentStatus.InTransit],
    status: ShipmentStatus.InTransit
  },
  {
    label: 'Delivered',
    value: shipmentsStore.statusDistribution.delivered,
    color: SHIPMENT_STATUS_COLORS[ShipmentStatus.Delivered],
    status: ShipmentStatus.Delivered
  },
  {
    label: 'Delayed',
    value: shipmentsStore.statusDistribution.delayed,
    color: SHIPMENT_STATUS_COLORS[ShipmentStatus.Delayed],
    status: ShipmentStatus.Delayed
  }
]);

// Methods
const handleTrackShipment = async () => {
  if (trackingInput.value.trim()) {
    await shipmentsStore.trackShipment(trackingInput.value);
  }
};

const trackSpecificShipment = async (trackingNumber: string) => {
  trackingInput.value = trackingNumber;
  await shipmentsStore.trackShipment(trackingNumber);
};

const setStatusFilter = async (status?: number) => {
  filters.status = status;
  await shipmentsStore.setStatusFilter(status);
};

const updateStatusFilter = async () => {
  await shipmentsStore.setStatusFilter(filters.status);
};

const clearFilters = async () => {
  filters.status = undefined;
  await shipmentsStore.clearFilters();
};

const goToPage = async (page: number) => {
  await shipmentsStore.goToPage(page);
};

const getStatusClass = (status: number): string => {
  switch (status) {
    case ShipmentStatus.Processing: return 'status-processing';
    case ShipmentStatus.InTransit: return 'status-in-transit';
    case ShipmentStatus.Delivered: return 'status-delivered';
    case ShipmentStatus.Delayed: return 'status-delayed';
    default: return 'status-unknown';
  }
};

const formatDate = (dateString: string): string => {
  const date = new Date(dateString);
  return date.toLocaleDateString('en-GB', {
    day: '2-digit',
    month: 'short',
    year: 'numeric',
    hour: '2-digit',
    minute: '2-digit'
  });
};

// Initialize data on mount
onMounted(async () => {
  await shipmentsStore.initialize();
});
</script>

<style scoped>
/* Shipment-specific styles only */
/* Most styles now come from centralized CSS */

/* Shipment tracking specific components */
.tracking-card {
  padding: var(--spacing-2xl);
}

.input-group {
  display: flex;
  gap: var(--spacing-lg);
  margin-bottom: var(--spacing-lg);
}

.tracking-details {
  background: var(--color-gray-50);
  border-radius: var(--border-radius-md);
  padding: var(--spacing-xl);
  margin-top: var(--spacing-xl);
}

.detail-row {
  display: flex;
  justify-content: space-between;
  padding: var(--spacing-sm) 0;
  border-bottom: var(--border-width) solid var(--color-gray-200);
}

.detail-row:last-child {
  border-bottom: none;
}

.detail-row .label {
  font-weight: var(--font-weight-semibold);
  color: var(--color-text-secondary);
  min-width: 150px;
}

.detail-row .value {
  color: var(--color-text-primary);
  text-align: right;
}

/* Table rows are clickable for tracking */
.grid-table-row {
  cursor: pointer;
}

/* Responsive adjustments for shipment-specific components */
@media (max-width: 768px) {
  .input-group {
    flex-direction: column;
  }

  .tracking-card {
    padding: var(--spacing-xl);
  }
}
</style>
