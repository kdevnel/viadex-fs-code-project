<template>
  <div class="page-view">
    <!-- Header Section -->
    <div class="page-header">
      <h1 class="page-title">Device Management</h1>
      <p class="page-subtitle">Manage your device inventory with status tracking and analytics</p>
    </div>



    <!-- Status Overview Section -->
    <div class="stats-section section-spacing">
      <div class="stats-grid">
        <div class="stat-card">
          <div class="stat-value">{{ deviceStore.totalDevices }}</div>
          <div class="stat-label">Total Devices</div>
        </div>
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

    <!-- Devices List Section -->
    <div class="content-section">
      <div class="list-header">
        <h2 class="section-title">All Devices</h2>
        <div class="list-controls">
          <select v-model="filters.status" @change="updateStatusFilter" class="status-filter">
            <option :value="undefined">All Statuses</option>
            <option :value="1">Active</option>
            <option :value="2">Retired</option>
            <option :value="3">Under Repair</option>
          </select>
          <button v-if="deviceStore.activeFiltersCount > 0" @click="clearFilters" class="clear-filters">
            Clear Filters ({{ deviceStore.activeFiltersCount }})
          </button>
          <button class="btn btn-primary" @click="showCreateModal = true">
            Add Device
          </button>
        </div>
      </div>
      <!-- Error State -->
      <div v-if="deviceStore.error" class="error-banner">
        <div class="error-content">
          <span class="error-icon">⚠️</span>
          <span class="error-message">{{ deviceStore.error }}</span>
          <button
            class="error-retry"
            @click="handleRetry"
          >
            Retry
          </button>
        </div>
      </div>

      <!-- Loading State -->
      <div v-if="deviceStore.loading" class="loading-state">
        <div class="loading-spinner"></div>
        <p>Loading devices...</p>
      </div>

      <!-- Empty State -->
      <div v-else-if="!deviceStore.hasDevices" class="empty-state">
        <div class="empty-icon">📱</div>
        <h3>No devices found</h3>
        <p v-if="deviceStore.activeFiltersCount > 0">
          Try adjusting your filters to see more results.
        </p>
        <p v-else>
          Devices will appear here once they are created.
        </p>
      </div>

      <!-- Devices Table -->
      <div v-else class="grid-table">
        <div class="grid-table-header grid-6-cols">
          <div>Name</div>
          <div>Model</div>
          <div>Status</div>
          <div>Monthly Price</div>
          <div>Purchase Date</div>
          <div>Actions</div>
        </div>

        <div
          v-for="device in deviceStore.currentPageDevices"
          :key="device.id"
          class="grid-table-row grid-6-cols"
        >
          <div class="cell-name">
            <strong>{{ device.name }}</strong>
          </div>
          <div class="cell-secondary">{{ device.model }}</div>
          <div>
            <span :class="['status-badge', getStatusClass(device.status)]">
              {{ device.statusName }}
            </span>
          </div>
          <div class="cell-secondary">£{{ formatPrice(device.monthlyPrice) }}/month</div>
          <div class="cell-secondary">{{ formatDate(device.purchaseDate) }}</div>
          <div class="cell-actions">
            <button
              class="btn btn-small btn-secondary"
              @click="handleViewDevice(device)"
            >
              View
            </button>
            <button
              class="btn btn-small btn-outline"
              @click="handleEditDevice(device)"
            >
              Edit
            </button>
            <button
              class="btn btn-small btn-danger"
              @click="handleDeleteDevice(device)"
            >
              Delete
            </button>
          </div>
        </div>
      </div>

      <!-- Pagination -->
      <div v-if="deviceStore.totalPages > 1" class="pagination">
        <button
          @click="deviceStore.goToPage(deviceStore.filters.page - 1)"
          :disabled="deviceStore.filters.page <= 1"
          class="page-button"
        >
          Previous
        </button>

        <span class="page-info">
          Page {{ deviceStore.filters.page }} of {{ deviceStore.totalPages }}
          ({{ deviceStore.totalDevices }} total)
        </span>

        <button
          @click="deviceStore.goToPage(deviceStore.filters.page + 1)"
          :disabled="deviceStore.filters.page >= deviceStore.totalPages"
          class="page-button"
        >
          Next
        </button>
      </div>
    </div>

    <!-- Create Device Modal -->
    <div v-if="showCreateModal" class="modal-overlay" @click="closeCreateModal">
      <div class="modal-content" @click.stop>
        <div class="modal-header">
          <h2>Add New Device</h2>
          <button class="modal-close" @click="closeCreateModal">×</button>
        </div>
        <form @submit.prevent="handleCreateDevice" class="modal-form">
          <div class="form-group">
            <label for="deviceName">Device Name</label>
            <input
              id="deviceName"
              v-model="newDevice.name"
              type="text"
              placeholder="Enter device name"
              required
              class="form-input"
            />
          </div>
          <div class="form-group">
            <label for="deviceModel">Model</label>
            <input
              id="deviceModel"
              v-model="newDevice.model"
              type="text"
              placeholder="Enter device model"
              required
              class="form-input"
            />
          </div>
          <div class="form-group">
            <label for="devicePrice">Monthly Price (£)</label>
            <input
              id="devicePrice"
              v-model.number="newDevice.monthlyPrice"
              type="number"
              step="0.01"
              min="0"
              max="10000"
              placeholder="0.00"
              required
              class="form-input"
            />
          </div>
          <div class="modal-actions">
            <button type="button" class="btn btn-outline" @click="closeCreateModal">
              Cancel
            </button>
            <button type="submit" class="btn btn-primary" :disabled="createLoading">
              {{ createLoading ? 'Creating...' : 'Create Device' }}
            </button>
          </div>
        </form>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted, reactive } from 'vue';
import { useDevicesStore } from '@/stores/useDevices';
import type { Device, DeviceCreateRequest } from '@/services/deviceApi';
import { DeviceStatus, DEVICE_STATUS_COLORS } from '@/types/device';

// Component name for Vue DevTools
defineOptions({
  name: 'DevicesView'
});

const deviceStore = useDevicesStore();

// Local state
const showCreateModal = ref(false);
const createLoading = ref(false);

// Reactive filters that sync with store
const filters = reactive({
  status: deviceStore.filters.status
});

// New device form
const newDevice = ref<DeviceCreateRequest>({
  name: '',
  model: '',
  monthlyPrice: 0,
});

// Status statistics for overview cards
const statusStats = computed(() => [
  {
    label: 'Active',
    value: deviceStore.statusDistribution.active,
    color: DEVICE_STATUS_COLORS[DeviceStatus.Active],
    status: DeviceStatus.Active
  },
  {
    label: 'Retired',
    value: deviceStore.statusDistribution.retired,
    color: DEVICE_STATUS_COLORS[DeviceStatus.Retired],
    status: DeviceStatus.Retired
  },
  {
    label: 'Under Repair',
    value: deviceStore.statusDistribution.underRepair,
    color: DEVICE_STATUS_COLORS[DeviceStatus.UnderRepair],
    status: DeviceStatus.UnderRepair
  }
]);

// Methods
const setStatusFilter = async (status?: number) => {
  filters.status = status;
  await deviceStore.setStatusFilter(status);
};

const updateStatusFilter = async () => {
  await deviceStore.setStatusFilter(filters.status);
};

const clearFilters = async () => {
  filters.status = undefined;
  await deviceStore.clearFilters();
};

const handleRetry = () => {
  deviceStore.clearError();
  deviceStore.fetchDevices();
};

const handleViewDevice = (device: Device) => {
  deviceStore.selectDevice(device);
  // Navigate to detail view or show modal
  alert('View device: ' + device.name);
};

const handleEditDevice = (device: Device) => {
  // Navigate to edit form or show modal
  alert('Edit device: ' + device.name);
};

const handleDeleteDevice = async (device: Device) => {
  if (confirm(`Are you sure you want to delete "${device.name}"?`)) {
    const result = await deviceStore.deleteDevice(device.id);
    if (!result.success) {
      alert('Failed to delete device: ' + result.error);
    }
  }
};

const handleCreateDevice = async () => {
  createLoading.value = true;

  try {
    const result = await deviceStore.addDevice(newDevice.value);

    if (result.success) {
      closeCreateModal();
      resetNewDevice();
    } else {
      alert('Failed to create device: ' + result.error);
    }
  } finally {
    createLoading.value = false;
  }
};

const closeCreateModal = () => {
  showCreateModal.value = false;
  resetNewDevice();
};

const resetNewDevice = () => {
  newDevice.value = {
    name: '',
    model: '',
    monthlyPrice: 0,
  };
};

// Utility functions
const getStatusClass = (status?: number | string) => {
  const baseClass = 'status-badge';
  // Convert numeric status to string for comparison
  let statusName: string;
  if (typeof status === 'number') {
    switch (status) {
      case 1: statusName = 'Active'; break;
      case 2: statusName = 'Retired'; break;
      case 3: statusName = 'UnderRepair'; break;
      default: statusName = 'Unknown';
    }
  } else {
    statusName = status || 'Unknown';
  }

  switch (statusName) {
    case 'Active': return `${baseClass} status-active`;
    case 'Retired': return `${baseClass} status-retired`;
    case 'UnderRepair': return `${baseClass} status-repair`;
    default: return `${baseClass} status-unknown`;
  }
};

const formatPrice = (price: number) => {
  return price.toFixed(2);
};

const formatDate = (dateString: string) => {
  return new Date(dateString).toLocaleDateString('en-GB', {
    year: 'numeric',
    month: 'short',
    day: 'numeric',
  });
};

// Initialize data on mount
onMounted(async () => {
  await deviceStore.fetchDevices();
});


</script>
