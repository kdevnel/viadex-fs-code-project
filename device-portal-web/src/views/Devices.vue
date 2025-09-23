<template>
  <div class="devices-view">
    <!-- Header Section -->
    <div class="header">
      <h1 class="title">Device Management</h1>
      <p class="subtitle">Manage your device inventory with status tracking and analytics</p>
    </div>

    <!-- Filter Controls -->
    <div class="filters-section">
      <div class="filters-row">
        <!-- Search Input -->
        <div class="filter-group">
          <label for="search" class="filter-label">Search</label>
          <input
            id="search"
            v-model="searchTerm"
            type="text"
            placeholder="Search devices by name or model..."
            class="filter-input"
            @input="handleSearchChange"
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
    <div class="stats-section">
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
        <table class="devices-table">
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
              <td class="device-name">{{ device.name }}</td>
              <td class="device-model">{{ device.model }}</td>
              <td class="device-status">
                <span :class="getStatusClass(device.status)">
                  {{ getStatusText(device.status) }}
                </span>
              </td>
              <td class="device-price">£{{ formatPrice(device.monthlyPrice) }}/month</td>
              <td class="device-date">{{ formatDate(device.purchaseDate) }}</td>
              <td class="device-actions">
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
  return searchTerm.value.trim() !== '' || statusFilter.value !== '';
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
  // Debounce search input
  clearTimeout(searchTimeout.value);
  searchTimeout.value = setTimeout(() => {
    deviceStore.updateFilters({ searchTerm: searchTerm.value, page: 1 });
  }, 300);
};

const searchTimeout = ref<ReturnType<typeof setTimeout>>();

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
  console.log('View device:', device);
};

const handleEditDevice = (device: Device) => {
  // Navigate to edit form or show modal
  console.log('Edit device:', device);
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

const getStatusText = (status?: string) => {
  switch (status) {
    case 'UnderRepair': return 'Under Repair';
    default: return status || 'Unknown';
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
.devices-view {
  min-height: 100vh;
  background-color: #f8fafc;
  padding: 2rem;
}

/* Header */
.header {
  margin-bottom: 2rem;
}

.title {
  font-size: 2.5rem;
  font-weight: 700;
  color: #1e293b;
  margin: 0 0 0.5rem 0;
}

.subtitle {
  font-size: 1.125rem;
  color: #64748b;
  margin: 0;
}

/* Filters */
.filters-section {
  margin-bottom: 2rem;
}

.filters-row {
  display: flex;
  gap: 1.5rem;
  align-items: end;
  flex-wrap: wrap;
}

.filter-group {
  display: flex;
  flex-direction: column;
  gap: 0.5rem;
}

.filter-label {
  font-size: 0.875rem;
  font-weight: 500;
  color: #374151;
}

.filter-input,
.filter-select {
  padding: 0.5rem 0.75rem;
  border: 1px solid #d1d5db;
  border-radius: 0.375rem;
  font-size: 0.875rem;
  min-width: 200px;
}

.filter-input:focus,
.filter-select:focus {
  outline: none;
  border-color: #3b82f6;
  box-shadow: 0 0 0 3px rgba(59, 130, 246, 0.1);
}

/* Stats */
.stats-section {
  margin-bottom: 2rem;
}

.stats-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
  gap: 1rem;
}

.stat-card {
  background: white;
  padding: 1.5rem;
  border-radius: 0.5rem;
  box-shadow: 0 1px 3px rgba(0, 0, 0, 0.1);
}

.stat-value {
  font-size: 2rem;
  font-weight: 700;
  color: #1e293b;
}

.stat-label {
  font-size: 0.875rem;
  color: #64748b;
  margin-top: 0.25rem;
}

/* Content */
.content-section {
  background: white;
  border-radius: 0.5rem;
  box-shadow: 0 1px 3px rgba(0, 0, 0, 0.1);
  overflow: hidden;
}

/* Error Banner */
.error-banner {
  background-color: #fef2f2;
  border-left: 4px solid #ef4444;
  padding: 1rem;
}

.error-content {
  display: flex;
  align-items: center;
  gap: 0.75rem;
}

.error-icon {
  font-size: 1.25rem;
}

.error-message {
  flex: 1;
  color: #dc2626;
  font-weight: 500;
}

.error-retry {
  background: #dc2626;
  color: white;
  border: none;
  padding: 0.5rem 1rem;
  border-radius: 0.25rem;
  font-size: 0.875rem;
  cursor: pointer;
}

/* Loading */
.loading-state {
  padding: 4rem;
  text-align: center;
  color: #64748b;
}

.loading-spinner {
  width: 2rem;
  height: 2rem;
  border: 2px solid #e5e7eb;
  border-top: 2px solid #3b82f6;
  border-radius: 50%;
  animation: spin 1s linear infinite;
  margin: 0 auto 1rem;
}

@keyframes spin {
  to { transform: rotate(360deg); }
}

/* Empty State */
.empty-state {
  padding: 4rem;
  text-align: center;
  color: #64748b;
}

.empty-icon {
  font-size: 4rem;
  margin-bottom: 1rem;
}

.empty-state h3 {
  color: #374151;
  margin-bottom: 0.5rem;
}

/* Table */
.table-container {
  overflow-x: auto;
}

.devices-table {
  width: 100%;
  border-collapse: collapse;
}

.devices-table th,
.devices-table td {
  padding: 1rem;
  text-align: left;
  border-bottom: 1px solid #e5e7eb;
}

.devices-table th {
  background-color: #f9fafb;
  font-weight: 600;
  color: #374151;
  font-size: 0.875rem;
}

.devices-table tr:hover {
  background-color: #f9fafb;
}

.device-name {
  font-weight: 500;
  color: #1e293b;
}

.device-model {
  color: #64748b;
}

.status-badge {
  display: inline-block;
  padding: 0.25rem 0.75rem;
  border-radius: 9999px;
  font-size: 0.75rem;
  font-weight: 500;
  text-transform: uppercase;
  letter-spacing: 0.05em;
}

.status-active {
  background-color: #dcfce7;
  color: #166534;
}

.status-retired {
  background-color: #f3f4f6;
  color: #374151;
}

.status-repair {
  background-color: #fef3c7;
  color: #92400e;
}

.status-unknown {
  background-color: #f1f5f9;
  color: #64748b;
}

.device-price {
  font-weight: 500;
  color: #059669;
}

.device-actions {
  display: flex;
  gap: 0.5rem;
}

/* Pagination */
.pagination-section {
  display: flex;
  justify-content: between;
  align-items: center;
  padding: 1rem;
  border-top: 1px solid #e5e7eb;
  gap: 1rem;
}

.pagination-controls {
  display: flex;
  align-items: center;
  gap: 0.5rem;
}

.page-numbers {
  display: flex;
  gap: 0.25rem;
}

/* Buttons */
.btn {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  padding: 0.5rem 1rem;
  border-radius: 0.375rem;
  font-size: 0.875rem;
  font-weight: 500;
  border: 1px solid transparent;
  cursor: pointer;
  transition: all 0.15s ease;
  text-decoration: none;
}

.btn:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}

.btn-primary {
  background-color: #3b82f6;
  color: white;
  border-color: #3b82f6;
}

.btn-primary:hover:not(:disabled) {
  background-color: #2563eb;
  border-color: #2563eb;
}

.btn-secondary {
  background-color: #6b7280;
  color: white;
  border-color: #6b7280;
}

.btn-outline {
  background-color: transparent;
  color: #374151;
  border-color: #d1d5db;
}

.btn-outline:hover:not(:disabled) {
  background-color: #f9fafb;
  border-color: #9ca3af;
}

.btn-danger {
  background-color: #ef4444;
  color: white;
  border-color: #ef4444;
}

.btn-danger:hover:not(:disabled) {
  background-color: #dc2626;
  border-color: #dc2626;
}

.btn-small {
  padding: 0.25rem 0.75rem;
  font-size: 0.75rem;
}

/* Modal */
.modal-overlay {
  position: fixed;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background-color: rgba(0, 0, 0, 0.5);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 1000;
}

.modal-content {
  background: white;
  border-radius: 0.5rem;
  box-shadow: 0 25px 50px -12px rgba(0, 0, 0, 0.25);
  width: 90%;
  max-width: 500px;
  max-height: 90vh;
  overflow-y: auto;
}

.modal-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 1.5rem;
  border-bottom: 1px solid #e5e7eb;
}

.modal-header h2 {
  margin: 0;
  font-size: 1.25rem;
  font-weight: 600;
  color: #1e293b;
}

.modal-close {
  background: none;
  border: none;
  font-size: 1.5rem;
  cursor: pointer;
  color: #6b7280;
  padding: 0;
  width: 2rem;
  height: 2rem;
  display: flex;
  align-items: center;
  justify-content: center;
  border-radius: 0.25rem;
}

.modal-close:hover {
  background-color: #f3f4f6;
  color: #374151;
}

.modal-form {
  padding: 1.5rem;
}

.form-group {
  margin-bottom: 1rem;
}

.form-group label {
  display: block;
  margin-bottom: 0.5rem;
  font-size: 0.875rem;
  font-weight: 500;
  color: #374151;
}

.form-input {
  width: 100%;
  padding: 0.5rem 0.75rem;
  border: 1px solid #d1d5db;
  border-radius: 0.375rem;
  font-size: 0.875rem;
  box-sizing: border-box;
}

.form-input:focus {
  outline: none;
  border-color: #3b82f6;
  box-shadow: 0 0 0 3px rgba(59, 130, 246, 0.1);
}

.modal-actions {
  display: flex;
  gap: 0.75rem;
  justify-content: flex-end;
  margin-top: 1.5rem;
}

/* Responsive */
@media (max-width: 768px) {
  .devices-view {
    padding: 1rem;
  }

  .filters-row {
    flex-direction: column;
    align-items: stretch;
  }

  .filter-input,
  .filter-select {
    min-width: auto;
  }

  .stats-grid {
    grid-template-columns: repeat(2, 1fr);
  }

  .pagination-section {
    flex-direction: column;
    gap: 1rem;
  }

  .device-actions {
    flex-direction: column;
  }

  .modal-content {
    margin: 1rem;
    width: auto;
  }
}
</style>
