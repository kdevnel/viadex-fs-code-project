<template>
  <div class="page-view">
    <!-- Header Section -->
    <div class="page-header">
      <h1 class="page-title">Shipment Tracker</h1>
      <p class="page-subtitle">Track shipments by number and monitor delivery status across your supply chain</p>
    </div>

    <!-- Tracking Section -->
    <div class="content-section">
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

          <!-- Sample tracking numbers for testing -->
          <div class="sample-tracking">
            <small class="sample-label">Try these sample tracking numbers:</small>
            <div class="sample-numbers">
              <button
                v-for="sample in sampleTrackingNumbers"
                :key="sample.number"
                @click="trackingInput = sample.number"
                class="sample-button"
                :class="sample.statusClass"
              >
                {{ sample.number }}
                <span class="sample-status">({{ sample.status }})</span>
              </button>
            </div>
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
    <div class="content-section">
      <div class="overview-cards">
        <h2 class="section-title">Shipment Overview</h2>
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
    </div>

    <!-- Shipments List Section -->
    <div class="content-section">
      <div class="list-header">
        <h2 class="section-title">All Shipments</h2>
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
      <div v-else class="shipments-table">
        <div class="table-header">
          <div class="col-tracking">Tracking Number</div>
          <div class="col-customer">Customer</div>
          <div class="col-status">Status</div>
          <div class="col-destination">Destination</div>
          <div class="col-eta">Est. Delivery</div>
          <div class="col-created">Created</div>
        </div>

        <div
          v-for="shipment in shipmentsStore.currentPageShipments"
          :key="shipment.id"
          class="table-row"
          @click="trackSpecificShipment(shipment.trackingNumber)"
        >
          <div class="col-tracking">
            <strong>{{ shipment.trackingNumber }}</strong>
          </div>
          <div class="col-customer">{{ shipment.customerName }}</div>
          <div class="col-status">
            <span :class="['status-badge', getStatusClass(shipment.status)]">
              {{ shipment.statusName }}
            </span>
          </div>
          <div class="col-destination">{{ shipment.destination }}</div>
          <div class="col-eta">{{ formatDate(shipment.estimatedDelivery) }}</div>
          <div class="col-created">{{ formatDate(shipment.createdAt) }}</div>
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
import { ShipmentStatus } from '@/types/shipment';

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

// Sample tracking numbers for easy testing
const sampleTrackingNumbers = [
  { number: 'TRK001234567', status: 'In Transit', statusClass: 'sample-in-transit' },
  { number: 'TRK002345678', status: 'Delivered', statusClass: 'sample-delivered' },
  { number: 'TRK003456789', status: 'Processing', statusClass: 'sample-processing' },
  { number: 'TRK004567890', status: 'Delayed', statusClass: 'sample-delayed' }
];

// Status statistics for overview cards
const statusStats = computed(() => [
  {
    label: 'Processing',
    value: shipmentsStore.statusDistribution.processing,
    color: '#3b82f6',
    status: ShipmentStatus.Processing
  },
  {
    label: 'In Transit',
    value: shipmentsStore.statusDistribution.inTransit,
    color: '#f59e0b',
    status: ShipmentStatus.InTransit
  },
  {
    label: 'Delivered',
    value: shipmentsStore.statusDistribution.delivered,
    color: '#10b981',
    status: ShipmentStatus.Delivered
  },
  {
    label: 'Delayed',
    value: shipmentsStore.statusDistribution.delayed,
    color: '#ef4444',
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
/* Shipment Tracker Specific Styles */

.tracking-card {
  background: white;
  border-radius: 12px;
  padding: 2rem;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
  margin-bottom: 2rem;
}

.tracking-form {
  margin-top: 1rem;
}

.input-group {
  display: flex;
  gap: 1rem;
  max-width: 600px;
}

.tracking-input {
  flex: 1;
  padding: 0.75rem 1rem;
  border: 2px solid #e2e8f0;
  border-radius: 8px;
  font-size: 1rem;
  transition: all 0.2s ease;
}

.tracking-input:focus {
  outline: none;
  border-color: #3b82f6;
  box-shadow: 0 0 0 3px rgba(59, 130, 246, 0.1);
}

.tracking-input:disabled {
  background-color: #f8fafc;
  cursor: not-allowed;
}

.track-button {
  padding: 0.75rem 2rem;
  background: #3b82f6;
  color: white;
  border: none;
  border-radius: 8px;
  font-weight: 600;
  cursor: pointer;
  transition: all 0.2s ease;
  white-space: nowrap;
}

.track-button:hover:not(:disabled) {
  background: #2563eb;
  transform: translateY(-1px);
}

.track-button:disabled {
  background: #cbd5e1;
  cursor: not-allowed;
  transform: none;
}

.sample-tracking {
  margin-top: 1rem;
  padding: 1rem;
  background: #f8fafc;
  border-radius: 8px;
  border: 1px solid #e2e8f0;
}

.sample-label {
  color: #64748b;
  font-weight: 500;
  display: block;
  margin-bottom: 0.5rem;
}

.sample-numbers {
  display: flex;
  flex-wrap: wrap;
  gap: 0.5rem;
}

.sample-button {
  padding: 0.5rem 0.75rem;
  border: 1px solid #d1d5db;
  border-radius: 6px;
  background: white;
  cursor: pointer;
  font-size: 0.8rem;
  transition: all 0.2s ease;
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 0.25rem;
}

.sample-button:hover {
  transform: translateY(-1px);
  box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
}

.sample-status {
  font-size: 0.7rem;
  opacity: 0.7;
}

.sample-in-transit:hover {
  border-color: #f59e0b;
  background: rgba(245, 158, 11, 0.05);
}

.sample-delivered:hover {
  border-color: #10b981;
  background: rgba(16, 185, 129, 0.05);
}

.sample-processing:hover {
  border-color: #3b82f6;
  background: rgba(59, 130, 246, 0.05);
}

.sample-delayed:hover {
  border-color: #ef4444;
  background: rgba(239, 68, 68, 0.05);
}

.tracking-result {
  margin-top: 1.5rem;
  padding: 1.5rem;
  background: #f8fafc;
  border-radius: 8px;
  border-left: 4px solid #10b981;
}

.result-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 1rem;
}

.result-header h3 {
  margin: 0;
  color: #1e293b;
}

.clear-button {
  background: none;
  border: none;
  font-size: 1.2rem;
  color: #64748b;
  cursor: pointer;
  padding: 0.25rem;
  border-radius: 4px;
  transition: all 0.2s ease;
}

.clear-button:hover {
  background: #e2e8f0;
  color: #334155;
}

.shipment-details {
  display: grid;
  gap: 0.75rem;
}

.detail-row {
  display: flex;
  justify-content: space-between;
  padding: 0.5rem 0;
  border-bottom: 1px solid #e2e8f0;
}

.detail-row:last-child {
  border-bottom: none;
}

.detail-row .label {
  font-weight: 600;
  color: #64748b;
  min-width: 150px;
}

.detail-row .value {
  color: #1e293b;
  text-align: right;
}

.overview-cards {
  background: white;
  border-radius: 12px;
  padding: 2rem;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
  margin-bottom: 2rem;
}

.stats-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
  gap: 1rem;
  margin-top: 1rem;
}

.stat-card {
  padding: 1.5rem;
  background: #f8fafc;
  border-radius: 8px;
  text-align: center;
  cursor: pointer;
  transition: all 0.2s ease;
  border: 2px solid transparent;
}

.stat-card:hover {
  transform: translateY(-2px);
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.1);
}

.stat-card.active {
  border-color: #3b82f6;
  background: rgba(59, 130, 246, 0.05);
}

.stat-value {
  font-size: 2.5rem;
  font-weight: 700;
  margin-bottom: 0.5rem;
}

.stat-label {
  font-size: 0.9rem;
  color: #64748b;
  font-weight: 500;
}

.list-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 1.5rem;
}

.list-controls {
  display: flex;
  gap: 1rem;
  align-items: center;
}

.status-filter {
  padding: 0.5rem 1rem;
  border: 2px solid #e2e8f0;
  border-radius: 8px;
  background: white;
  font-size: 0.9rem;
  cursor: pointer;
}

.status-filter:focus {
  outline: none;
  border-color: #3b82f6;
}

.clear-filters {
  padding: 0.5rem 1rem;
  background: #f1f5f9;
  border: 1px solid #cbd5e1;
  border-radius: 6px;
  font-size: 0.85rem;
  color: #475569;
  cursor: pointer;
  transition: all 0.2s ease;
}

.clear-filters:hover {
  background: #e2e8f0;
}

.shipments-table {
  background: white;
  border-radius: 8px;
  overflow: hidden;
  box-shadow: 0 1px 3px rgba(0, 0, 0, 0.1);
}

.table-header {
  display: grid;
  grid-template-columns: 1.5fr 1fr 1fr 2fr 1.2fr 1fr;
  gap: 1rem;
  padding: 1rem;
  background: #f8fafc;
  font-weight: 600;
  color: #374151;
  border-bottom: 1px solid #e5e7eb;
}

.table-row {
  display: grid;
  grid-template-columns: 1.5fr 1fr 1fr 2fr 1.2fr 1fr;
  gap: 1rem;
  padding: 1rem;
  border-bottom: 1px solid #f3f4f6;
  cursor: pointer;
  transition: background-color 0.2s ease;
}

.table-row:hover {
  background: #f8fafc;
}

.table-row:last-child {
  border-bottom: none;
}

.col-tracking strong {
  color: #3b82f6;
}

.col-destination {
  font-size: 0.9rem;
  color: #6b7280;
}

.pagination {
  display: flex;
  justify-content: center;
  align-items: center;
  gap: 1rem;
  margin-top: 2rem;
  padding: 1rem;
}

.page-button {
  padding: 0.5rem 1rem;
  background: white;
  border: 1px solid #d1d5db;
  border-radius: 6px;
  cursor: pointer;
  transition: all 0.2s ease;
}

.page-button:hover:not(:disabled) {
  background: #f9fafb;
  border-color: #9ca3af;
}

.page-button:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}

.page-info {
  font-size: 0.9rem;
  color: #6b7280;
}

/* Status Badge Variations */
.status-badge {
  padding: 0.25rem 0.75rem;
  border-radius: 20px;
  font-size: 0.75rem;
  font-weight: 600;
  text-transform: uppercase;
  letter-spacing: 0.05em;
}

.status-processing {
  background: rgba(59, 130, 246, 0.1);
  color: #1d4ed8;
}

.status-in-transit {
  background: rgba(245, 158, 11, 0.1);
  color: #d97706;
}

.status-delivered {
  background: rgba(16, 185, 129, 0.1);
  color: #047857;
}

.status-delayed {
  background: rgba(239, 68, 68, 0.1);
  color: #dc2626;
}

.status-unknown {
  background: rgba(107, 114, 128, 0.1);
  color: #4b5563;
}

/* Responsive Design */
@media (max-width: 768px) {
  .input-group {
    flex-direction: column;
  }

  .stats-grid {
    grid-template-columns: repeat(2, 1fr);
  }

  .list-header {
    flex-direction: column;
    gap: 1rem;
    align-items: stretch;
  }

  .table-header,
  .table-row {
    grid-template-columns: 1fr;
    text-align: left;
  }

  .table-header > div,
  .table-row > div {
    padding: 0.5rem 0;
  }

  .table-header > div::before,
  .table-row > div::before {
    content: attr(data-label) ': ';
    font-weight: 600;
    color: #64748b;
  }
}
</style>
