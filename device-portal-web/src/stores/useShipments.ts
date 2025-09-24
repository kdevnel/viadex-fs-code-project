// Shipment store using native fetch API client
import { defineStore } from 'pinia';
import { ref, computed } from 'vue';
import { shipmentApi } from '@/services/shipmentApi';
import type {
  Shipment,
  ShipmentCreateRequest,
  ShipmentStatusDistribution,
  ShipmentStatus
} from '@/types/shipment';
import { SHIPMENT_STATUS_LABELS } from '@/types/shipment';

interface ShipmentFilters {
  page: number;
  pageSize: number;
  status?: number;
  searchTerm: string;
}

export const useShipmentsStore = defineStore('shipments', () => {
  // State
  const shipments = ref<Shipment[]>([]);
  const totalShipments = ref(0);
  const loading = ref(false);
  const error = ref<string | null>(null);
  const trackingResult = ref<Shipment | null>(null);
  const trackingLoading = ref(false);
  const trackingError = ref<string | null>(null);

  // Persisted filter state
  const filters = ref<ShipmentFilters>({
    page: 1,
    pageSize: 5,
    status: undefined,
    searchTerm: ''
  });

  // Status distribution for charts
  const rawStatusDistribution = ref<ShipmentStatusDistribution>({
    processing: 0,
    inTransit: 0,
    delivered: 0,
    delayed: 0
  });

  // Getters
  const currentPageShipments = computed(() => shipments.value);

  const totalPages = computed(() => Math.ceil(totalShipments.value / filters.value.pageSize));

  const statusDistribution = computed(() => rawStatusDistribution.value);

  const statusDistributionArray = computed(() => [
    { label: 'Processing', value: rawStatusDistribution.value.processing, color: '#3b82f6' },
    { label: 'In Transit', value: rawStatusDistribution.value.inTransit, color: '#f59e0b' },
    { label: 'Delivered', value: rawStatusDistribution.value.delivered, color: '#10b981' },
    { label: 'Delayed', value: rawStatusDistribution.value.delayed, color: '#ef4444' }
  ]);

  const hasShipments = computed(() => shipments.value.length > 0);

  const getStatusLabel = computed(() => (status: number) => SHIPMENT_STATUS_LABELS[status] || 'Unknown');

  const activeFiltersCount = computed(() => {
    let count = 0;
    if (filters.value.status) count++;
    if (filters.value.searchTerm.trim()) count++;
    return count;
  });

  // Actions
  const fetchShipments = async () => {
    loading.value = true;
    error.value = null;

    try {
      const response = await shipmentApi.getShipments({
        page: filters.value.page,
        pageSize: filters.value.pageSize,
        status: filters.value.status
      });

      if (response.isSuccess) {
        shipments.value = response.items;
        totalShipments.value = response.total;
      } else {
        error.value = response.errorMessage || 'Failed to fetch shipments';
        shipments.value = [];
        totalShipments.value = 0;
      }
    } catch (err) {
      error.value = err instanceof Error ? err.message : 'An unexpected error occurred';
      shipments.value = [];
      totalShipments.value = 0;
    } finally {
      loading.value = false;
    }
  };

  const trackShipment = async (trackingNumber: string) => {
    trackingLoading.value = true;
    trackingError.value = null;
    trackingResult.value = null;

    try {
      const shipment = await shipmentApi.trackShipment(trackingNumber.trim());

      if (shipment) {
        trackingResult.value = shipment;
      } else {
        trackingError.value = 'Shipment not found. Please check your tracking number.';
      }
    } catch (err) {
      trackingError.value = err instanceof Error ? err.message : 'Failed to track shipment';
    } finally {
      trackingLoading.value = false;
    }
  };

  const clearTrackingResult = () => {
    trackingResult.value = null;
    trackingError.value = null;
  };

  const fetchStatusDistribution = async () => {
    try {
      const distribution = await shipmentApi.getStatusDistribution();
      if (distribution) {
        rawStatusDistribution.value = distribution;
      }
    } catch (err) {
      console.error('Failed to fetch status distribution:', err);
    }
  };

  const updateFilters = async (newFilters: Partial<ShipmentFilters>) => {
    filters.value = { ...filters.value, ...newFilters };

    // Reset to page 1 when changing filters (except page)
    if ('status' in newFilters || 'searchTerm' in newFilters) {
      filters.value.page = 1;
    }

    await fetchShipments();
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

  const goToPage = async (page: number) => {
    if (page >= 1 && page <= totalPages.value) {
      await updateFilters({ page });
    }
  };

  const createShipment = async (shipmentData: ShipmentCreateRequest): Promise<boolean> => {
    try {
      const newShipment = await shipmentApi.createShipment(shipmentData);

      if (newShipment) {
        // Refresh the list to include the new shipment
        await fetchShipments();
        await fetchStatusDistribution();
        return true;
      }

      return false;
    } catch (err) {
      error.value = err instanceof Error ? err.message : 'Failed to create shipment';
      return false;
    }
  };

  const updateShipmentStatus = async (id: number, status: ShipmentStatus, actualDelivery?: string): Promise<boolean> => {
    try {
      const updatedShipment = await shipmentApi.updateShipmentStatus(id, {
        status,
        actualDelivery
      });

      if (updatedShipment) {
        // Update the shipment in the current list
        const index = shipments.value.findIndex(s => s.id === id);
        if (index !== -1) {
          shipments.value[index] = updatedShipment;
        }

        // Update tracking result if it matches
        if (trackingResult.value?.id === id) {
          trackingResult.value = updatedShipment;
        }

        // Refresh status distribution
        await fetchStatusDistribution();
        return true;
      }

      return false;
    } catch (err) {
      error.value = err instanceof Error ? err.message : 'Failed to update shipment status';
      return false;
    }
  };

  const initialize = async () => {
    await Promise.all([
      fetchShipments(),
      fetchStatusDistribution()
    ]);
  };

  return {
    // State
    shipments,
    totalShipments,
    loading,
    error,
    trackingResult,
    trackingLoading,
    trackingError,
    filters,

    // Getters
    currentPageShipments,
    totalPages,
    statusDistribution,
    statusDistributionArray,
    hasShipments,
    getStatusLabel,
    activeFiltersCount,

    // Actions
    fetchShipments,
    trackShipment,
    clearTrackingResult,
    fetchStatusDistribution,
    updateFilters,
    setStatusFilter,
    clearFilters,
    goToPage,
    createShipment,
    updateShipmentStatus,
    initialize
  };
}, {
  persist: true
});
