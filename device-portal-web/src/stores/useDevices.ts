// Device store using native fetch API client
import { defineStore } from 'pinia';
import { ref, computed } from 'vue';
import { deviceApi, type Device, type DeviceFilters, type DeviceCreateRequest } from '@/services/deviceApi';

export const useDevicesStore = defineStore('devices', () => {
  // State
  const devices = ref<Device[]>([]);
  const loading = ref(false);
  const error = ref<string | null>(null);
  const total = ref(0);
  const selectedDevice = ref<Device | null>(null);

  // Persisted filter state
  const filters = ref<DeviceFilters>({
    page: 1,
    pageSize: 20,
    status: '',
    searchTerm: '',
  });

  // Getters
  const statusDistribution = computed(() => {
    const distribution: Record<string, number> = {
      Active: 0,
      Retired: 0,
      UnderRepair: 0,
    };

    devices.value.forEach(device => {
      // Handle both numeric and string status values
      let statusKey: string | undefined;

      // Convert to number for comparison since backend returns numeric enum values
      const statusAsNumber = Number(device.status);
      const statusAsString = String(device.status);

      if (statusAsNumber === 1 || statusAsString === 'Active') {
        statusKey = 'Active';
      } else if (statusAsNumber === 2 || statusAsString === 'Retired') {
        statusKey = 'Retired';
      } else if (statusAsNumber === 3 || statusAsString === 'UnderRepair') {
        statusKey = 'UnderRepair';
      }

      if (statusKey && distribution.hasOwnProperty(statusKey)) {
        distribution[statusKey]++;
      }
    });

    return distribution;
  });

  const filteredDevices = computed(() => {
    let filtered = devices.value;

    if (filters.value.status) {
      filtered = filtered.filter(device => device.status === filters.value.status);
    }

    if (filters.value.searchTerm) {
      const search = filters.value.searchTerm.toLowerCase();
      filtered = filtered.filter(device =>
        device.name.toLowerCase().includes(search) ||
        device.model.toLowerCase().includes(search)
      );
    }

    return filtered;
  });

  const totalPages = computed(() => {
    return Math.ceil(total.value / (filters.value.pageSize || 20));
  });

  const hasNextPage = computed(() => {
    return (filters.value.page || 1) < totalPages.value;
  });

  const hasPreviousPage = computed(() => {
    return (filters.value.page || 1) > 1;
  });

  // Actions
  const fetchDevices = async () => {
    loading.value = true;
    error.value = null;

    try {
      const result = await deviceApi.getDevices(filters.value);

      if (result.isSuccess) {
        devices.value = result.items;
        total.value = result.total;
      } else {
        error.value = result.errorMessage || 'Failed to fetch devices';
        devices.value = [];
        total.value = 0;
      }
    } catch (err) {
      error.value = err instanceof Error ? err.message : 'Network error occurred';
      devices.value = [];
      total.value = 0;
    } finally {
      loading.value = false;
    }
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

  const createDevice = async (deviceData: DeviceCreateRequest) => {
    loading.value = true;
    error.value = null;

    try {
      const result = await deviceApi.createDevice(deviceData);

      if (result.device) {
        await fetchDevices();
        return { success: true, device: result.device };
      } else {
        error.value = result.error || 'Failed to create device';
        return { success: false, error: error.value };
      }
    } catch (err) {
      error.value = err instanceof Error ? err.message : 'Network error occurred';
      return { success: false, error: error.value };
    } finally {
      loading.value = false;
    }
  };

  const updateDevice = async (id: number, deviceData: Partial<DeviceCreateRequest>) => {
    loading.value = true;
    error.value = null;

    try {
      const result = await deviceApi.updateDevice(id, deviceData);

      if (result.device) {
        const index = devices.value.findIndex(d => d.id === id);
        if (index !== -1) {
          devices.value[index] = result.device;
        }
        return { success: true, device: result.device };
      } else {
        error.value = result.error || 'Failed to update device';
        return { success: false, error: error.value };
      }
    } catch (err) {
      error.value = err instanceof Error ? err.message : 'Network error occurred';
      return { success: false, error: error.value };
    } finally {
      loading.value = false;
    }
  };

  const deleteDevice = async (id: number) => {
    loading.value = true;
    error.value = null;

    try {
      const result = await deviceApi.deleteDevice(id);

      if (result.success) {
        devices.value = devices.value.filter(d => d.id !== id);
        total.value = Math.max(0, total.value - 1);
        return { success: true };
      } else {
        error.value = result.error || 'Failed to delete device';
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
    total.value = 0;
    selectedDevice.value = null;
    filters.value = {
      page: 1,
      pageSize: 20,
      status: '',
      searchTerm: '',
    };
  };

  return {
    // State
    devices,
    loading,
    error,
    total,
    selectedDevice,
    filters,

    // Getters
    statusDistribution,
    filteredDevices,
    totalPages,
    hasNextPage,
    hasPreviousPage,

    // Actions
    fetchDevices,
    updateFilters,
    nextPage,
    previousPage,
    goToPage,
    createDevice,
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
