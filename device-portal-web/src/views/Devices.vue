<template>
  <div class="page-view">
    <!-- Header Section -->
    <div class="page-header">
      <h1 class="page-title">Device Management</h1>
      <p class="page-subtitle">Manage your device inventory with status tracking and analytics</p>
    </div>

    <!-- Filter Controls -->
    <div class="filters-section">
      <div class="filters-row">
        <!-- Search Input -->
        <div class="filter-group">
          <label for="search" class="filter-label">Search <span class="not-implemented">(Demo - Not Implemented)</span></label>
          <input
            id="search"
            v-model="searchTerm"
            type="text"
            placeholder="Search devices by name..."
            class="filter-input filter-disabled"
            @input="handleSearchChange"
            disabled
          />
        </div>

        <!-- Status Filter -->
        <div class="filter-group">
          <label for="status" class="filter-label">Status</label>
          <select
            id="status"
            v-model="statusFilter"
            class="filter-select"
            @change="handleStatusChange"
            data-test="status-filter"
          >
            <option value="">All Statuses</option>
            <option value="Active">Active</option>
            <option value="Retired">Retired</option>
            <option value="UnderRepair">Under Repair</option>
          </select>
        </div>

        <!-- Page Size Filter -->
        <div class="filter-group">
          <label for="pageSize" class="filter-label">Items per page</label>
          <select
            id="pageSize"
            v-model="pageSize"
            class="filter-select"
            @change="handlePageSizeChange"
          >
            <option :value="5">5</option>
            <option :value="10">10</option>
            <option :value="20">20</option>
            <option :value="50">50</option>
          </select>
        </div>

        <!-- Add Device Button -->
        <div class="filter-group">
          <button
            class="btn btn-primary"
            @click="showCreateModal = true"
          >
            Add Device
          </button>
        </div>
      </div>
    </div>

    <!-- Stats Cards -->
    <div class="stats-section section-spacing">
      <div class="stats-grid">
        <div class="stat-card">
          <div class="stat-value">{{ deviceStore.total }}</div>
          <div class="stat-label">Total Devices</div>
        </div>
        <div class="stat-card">
          <div class="stat-value">{{ deviceStore.statusDistribution.Active }}</div>
          <div class="stat-label">Active</div>
        </div>
        <div class="stat-card">
          <div class="stat-value">{{ deviceStore.statusDistribution.Retired }}</div>
          <div class="stat-label">Retired</div>
        </div>
        <div class="stat-card">
          <div class="stat-value">{{ deviceStore.statusDistribution.UnderRepair }}</div>
          <div class="stat-label">Under Repair</div>
        </div>
      </div>
    </div>

    <!-- Main Content -->
    <div class="content-section">
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
      <div v-else-if="!deviceStore.loading && deviceStore.devices.length === 0" class="empty-state">
        <div class="empty-icon">📱</div>
        <h3>No devices found</h3>
        <p v-if="hasActiveFilters">Try adjusting your filters or search terms.</p>
        <p v-else>Get started by adding your first device.</p>
        <button
          v-if="!hasActiveFilters"
          class="btn btn-primary"
          @click="showCreateModal = true"
        >
          Add First Device
        </button>
      </div>

      <!-- Devices Table -->
      <div v-else class="table-container">
        <table class="data-table">
          <thead>
            <tr>
              <th>Name</th>
              <th>Model</th>
              <th>Status</th>
              <th>Monthly Price</th>
              <th>Purchase Date</th>
              <th>Actions</th>
            </tr>
          </thead>
          <tbody>
            <tr
              v-for="device in deviceStore.devices"
              :key="device.id"
              class="device-row"
            >
              <td class="cell-name">{{ device.name }}</td>
              <td class="cell-secondary">{{ device.model }}</td>
              <td class="device-status">
                <span :class="getStatusClass(device.status)">
                  {{ getStatusText(device.status) }}
                </span>
              </td>
              <td class="cell-price">£{{ formatPrice(device.monthlyPrice) }}/month</td>
              <td class="cell-secondary">{{ formatDate(device.purchaseDate) }}</td>
              <td class="cell-actions">
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
              </td>
            </tr>
          </tbody>
        </table>
      </div>

      <!-- Pagination -->
      <div v-if="deviceStore.devices.length > 0" class="pagination-section">
        <div class="pagination-info">
          Showing {{ getPaginationText() }}
        </div>
        <div class="pagination-controls">
          <button
            class="btn btn-outline btn-small"
            :disabled="!deviceStore.hasPreviousPage"
            @click="deviceStore.previousPage()"
          >
            Previous
          </button>

          <div class="page-numbers">
            <button
              v-for="page in visiblePages"
              :key="page"
              class="btn btn-small"
              :class="page === deviceStore.filters.page ? 'btn-primary' : 'btn-outline'"
              @click="deviceStore.goToPage(page)"
            >
              {{ page }}
            </button>
          </div>

          <button
            class="btn btn-outline btn-small"
            :disabled="!deviceStore.hasNextPage"
            @click="deviceStore.nextPage()"
          >
            Next
          </button>
        </div>
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
import { ref, computed, onMounted, watch } from 'vue';
import { useDevicesStore } from '@/stores/useDevices';
import type { Device, DeviceCreateRequest } from '@/services/deviceApi';

// Component name for Vue DevTools
defineOptions({
  name: 'DevicesView'
});

// Store
const deviceStore = useDevicesStore();

// Local reactive state
const searchTerm = ref(deviceStore.filters.searchTerm || '');
const statusFilter = ref(deviceStore.filters.status || '');
const pageSize = ref(deviceStore.filters.pageSize || 20);
const showCreateModal = ref(false);
const createLoading = ref(false);

// New device form
const newDevice = ref<DeviceCreateRequest>({
  name: '',
  model: '',
  monthlyPrice: 0,
});

// Computed properties
const hasActiveFilters = computed(() => {
  // Only consider status filter since search is not implemented
  return statusFilter.value !== '';
});

const visiblePages = computed(() => {
  const currentPage = deviceStore.filters.page || 1;
  const totalPages = deviceStore.totalPages;
  const pages: number[] = [];

  // Show up to 5 pages around current page
  const startPage = Math.max(1, currentPage - 2);
  const endPage = Math.min(totalPages, currentPage + 2);

  for (let i = startPage; i <= endPage; i++) {
    pages.push(i);
  }

  return pages;
});

// Methods
const handleSearchChange = () => {
  // Search is not implemented in this demo
  if (searchTerm.value.trim() !== '') {
    alert('Search functionality is not implemented in this demo version. Please use the status filter instead.');
    searchTerm.value = ''; // Clear the input
  }
};

const handleStatusChange = () => {
  deviceStore.updateFilters({ status: statusFilter.value, page: 1 });
};

const handlePageSizeChange = () => {
  deviceStore.updateFilters({ pageSize: pageSize.value, page: 1 });
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
    const result = await deviceStore.createDevice(newDevice.value);

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
const getStatusClass = (status?: string) => {
  const baseClass = 'status-badge';
  switch (status) {
    case 'Active': return `${baseClass} status-active`;
    case 'Retired': return `${baseClass} status-retired`;
    case 'UnderRepair': return `${baseClass} status-repair`;
    default: return `${baseClass} status-unknown`;
  }
};

const getStatusText = (status?: string | number) => {
  switch (status) {
    case 1:
    case 'Active':
      return 'Active';
    case 2:
    case 'Retired':
      return 'Retired';
    case 3:
    case 'UnderRepair':
      return 'Under Repair';
    default:
      return 'Unknown';
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

const getPaginationText = () => {
  const start = ((deviceStore.filters.page || 1) - 1) * (deviceStore.filters.pageSize || 20) + 1;
  const end = Math.min(start + (deviceStore.filters.pageSize || 20) - 1, deviceStore.total);
  return `${start}-${end} of ${deviceStore.total} devices`;
};

// Lifecycle
onMounted(() => {
  // Load devices on component mount
  deviceStore.fetchDevices();
});

// Watch for filter changes in store and sync with local state
watch(
  () => deviceStore.filters,
  (newFilters) => {
    searchTerm.value = newFilters.searchTerm || '';
    statusFilter.value = newFilters.status || '';
    pageSize.value = newFilters.pageSize || 20;
  },
  { deep: true }
);
</script>

<style scoped>
/* Only view-specific styles that can't be centralized remain here */

/* Custom pagination behavior specific to devices */
.pagination-section {
  justify-content: space-between;
}

/* Specific pagination info styling for devices view */
.pagination-info {
  font-style: italic;
}
</style>
