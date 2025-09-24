// Device store using native fetch API client
import { defineStore } from 'pinia';
import { ref, computed } from 'vue';
import { deviceApi } from '@/services/deviceApi';
import type {
  Device,
  DeviceCreateRequest,
  DeviceStatusDistribution
} from '@/types/device';
import { DEVICE_STATUS_LABELS } from '@/types/device';

interface DeviceFilters {
  page: number;
  pageSize: number;
  status?: number;
  searchTerm: string;
}

export const useDevicesStore = defineStore('devices', () => {
  // State
  const devices = ref<Device[]>([]);
  const totalDevices = ref(0);
  const loading = ref(false);
  const error = ref<string | null>(null);
  const selectedDevice = ref<Device | null>(null);

  // Persisted filter state
  const filters = ref<DeviceFilters>({
    page: 1,
    pageSize: 20,
    status: undefined,
    searchTerm: ''
  });

  // Status distribution for charts
  const rawStatusDistribution = ref<DeviceStatusDistribution>({
    active: 0,
    retired: 0,
    underRepair: 0
  });

    // Getters
  const currentPageDevices = computed(() => devices.value);

  const totalPages = computed(() => Math.ceil(totalDevices.value / filters.value.pageSize));

  const statusDistribution = computed(() => rawStatusDistribution.value);

  const statusDistributionArray = computed(() => [
    { label: 'Active', value: rawStatusDistribution.value.active, color: '#10b981' },
    { label: 'Retired', value: rawStatusDistribution.value.retired, color: '#6b7280' },
    { label: 'Under Repair', value: rawStatusDistribution.value.underRepair, color: '#f59e0b' }
  ]);

  const hasDevices = computed(() => devices.value.length > 0);

  const getStatusLabel = computed(() => (status: number) => DEVICE_STATUS_LABELS[status] || 'Unknown');

  const activeFiltersCount = computed(() => {
    let count = 0;
    if (filters.value.status) count++;
    if (filters.value.searchTerm.trim()) count++;
    return count;
  });

  const hasNextPage = computed(() => {
    return filters.value.page < totalPages.value;
  });

  const hasPreviousPage = computed(() => {
    return filters.value.page > 1;
  });

  // Actions
  const fetchDevices = async () => {
    loading.value = true;
    error.value = null;

    try {
      const response = await deviceApi.getDevices({
        page: filters.value.page,
        pageSize: filters.value.pageSize,
        status: filters.value.status?.toString()
      });

      if (response.isSuccess) {
        devices.value = response.items;
        totalDevices.value = response.total;
        updateStatusDistribution();
      } else {
        error.value = response.errorMessage || 'Failed to fetch devices';
        devices.value = [];
        totalDevices.value = 0;
      }
    } catch (err) {
      error.value = err instanceof Error ? err.message : 'An unexpected error occurred';
      devices.value = [];
      totalDevices.value = 0;
    } finally {
      loading.value = false;
    }
  };

  const updateStatusDistribution = () => {
    const distribution = { active: 0, retired: 0, underRepair: 0 };
    devices.value.forEach(device => {
      switch (device.status) {
        case 1: // Active
          distribution.active++;
          break;
        case 2: // Retired
          distribution.retired++;
          break;
        case 3: // Under Repair
          distribution.underRepair++;
          break;
      }
    });
    rawStatusDistribution.value = distribution;
  };

  const updateFilters = async (newFilters: Partial<DeviceFilters>) => {
    // Reset page when other filters change
    if (newFilters.status !== undefined || newFilters.searchTerm !== undefined) {
      filters.value.page = 1;
    }

    Object.assign(filters.value, newFilters);
    await fetchDevices();
  };

  const nextPage = async () => {
    if (hasNextPage.value) {
      filters.value.page = (filters.value.page || 1) + 1;
      await fetchDevices();
    }
  };

  const previousPage = async () => {
    if (hasPreviousPage.value) {
      filters.value.page = Math.max(1, (filters.value.page || 1) - 1);
      await fetchDevices();
    }
  };

  const goToPage = async (page: number) => {
    if (page >= 1 && page <= totalPages.value) {
      filters.value.page = page;
      await fetchDevices();
    }
  };

  const setStatusFilter = async (status?: number) => {
    await updateFilters({ status });
  };

  const clearFilters = async () => {
    await updateFilters({
      status: undefined,
      searchTerm: '',
      page: 1
    });
  };

  const addDevice = async (deviceData: DeviceCreateRequest): Promise<{ success: boolean; device?: Device; error?: string }> => {
    error.value = null;

    try {
      const device = await deviceApi.createDevice(deviceData);

      if (device) {
        devices.value.push(device);
        updateStatusDistribution();
        return { success: true, device };
      } else {
        error.value = 'Failed to create device';
        return { success: false, error: 'Failed to create device' };
      }
    } catch (err) {
      const errorMessage = err instanceof Error ? err.message : 'An unexpected error occurred';
      error.value = errorMessage;
      return { success: false, error: errorMessage };
    }
  };

    const updateDevice = async (id: number, deviceData: DeviceCreateRequest): Promise<{ success: boolean; device?: Device; error?: string }> => {
    error.value = null;

    try {
      const device = await deviceApi.updateDevice(id, deviceData);

      if (device) {
        const index = devices.value.findIndex((d: Device) => d.id === id);
        if (index !== -1) {
          devices.value[index] = device;
        }
        updateStatusDistribution();
        return { success: true, device };
      } else {
        error.value = 'Failed to update device';
        return { success: false, error: 'Failed to update device' };
      }
    } catch (err) {
      const errorMessage = err instanceof Error ? err.message : 'An unexpected error occurred';
      error.value = errorMessage;
      return { success: false, error: errorMessage };
    }
  };

  const deleteDevice = async (id: number) => {
    loading.value = true;
    error.value = null;

    try {
      const success = await deviceApi.deleteDevice(id);

      if (success) {
        devices.value = devices.value.filter((d: Device) => d.id !== id);
        totalDevices.value = Math.max(0, totalDevices.value - 1);
        updateStatusDistribution();
        return { success: true };
      } else {
        error.value = 'Failed to delete device';
        return { success: false, error: error.value };
      }
    } catch (err) {
      error.value = err instanceof Error ? err.message : 'Network error occurred';
      return { success: false, error: error.value };
    } finally {
      loading.value = false;
    }
  };

  const selectDevice = (device: Device | null) => {
    selectedDevice.value = device;
  };

  const clearError = () => {
    error.value = null;
  };

  const reset = () => {
    devices.value = [];
    loading.value = false;
    error.value = null;
    totalDevices.value = 0;
    selectedDevice.value = null;
    filters.value = {
      page: 1,
      pageSize: 20,
      status: undefined,
      searchTerm: ''
    };
    rawStatusDistribution.value = { active: 0, retired: 0, underRepair: 0 };
  };

  return {
    // State
    devices,
    totalDevices,
    loading,
    error,
    selectedDevice,
    filters,

    // Getters
    currentPageDevices,
    totalPages,
    statusDistribution,
    statusDistributionArray,
    hasDevices,
    getStatusLabel,
    activeFiltersCount,
    hasNextPage,
    hasPreviousPage,

    // Actions
    fetchDevices,
    updateFilters,
    nextPage,
    previousPage,
    goToPage,
    setStatusFilter,
    clearFilters,
    addDevice,
    updateDevice,
    deleteDevice,
    selectDevice,
    clearError,
    reset,
  };
}, {
  // Persist filters to localStorage
  persist: true,
});
