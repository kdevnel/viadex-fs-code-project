// Device API service using the native fetch client
import api from './api';
import type {
  Device,
  DeviceCreateRequest,
  DevicePagedResult,
  DeviceFilters
} from '@/types/device';
import { DEVICE_STATUS_LABELS } from '@/types/device';

// Re-export types for convenience
export type { Device, DeviceCreateRequest, DevicePagedResult, DeviceFilters };

export const deviceApi = {
  // Get all devices with optional filtering and pagination
  async getDevices(params?: {
    page?: number;
    pageSize?: number;
    status?: string;
    searchTerm?: string;
  }): Promise<DevicePagedResult> {
    const queryParams = new URLSearchParams();

    if (params?.page) queryParams.append('page', params.page.toString());
    if (params?.pageSize) queryParams.append('pageSize', params.pageSize.toString());
    if (params?.status) queryParams.append('status', params.status);
    if (params?.searchTerm) queryParams.append('searchTerm', params.searchTerm);

    const endpoint = `/api/devices${queryParams.toString() ? `?${queryParams.toString()}` : ''}`;

    try {
      type BackendDevice = Omit<Device, 'statusName'>;
      const response = await api.get<{ total: number; items: BackendDevice[] }>(endpoint);

      // Transform items to add statusName
      const transformedItems: Device[] = response.data.items.map(item => ({
        ...item,
        statusName: DEVICE_STATUS_LABELS[item.status] || 'Unknown'
      }));

      return {
        isSuccess: true,
        total: response.data.total,
        items: transformedItems
      };
    } catch (error) {
      console.error('Error fetching devices:', error);
      return {
        isSuccess: false,
        total: 0,
        items: [],
        errorMessage: error instanceof Error ? error.message : 'Failed to fetch devices'
      };
    }
  },

  // Get device by ID
  async getDeviceById(id: number): Promise<Device | null> {
    try {
      type BackendDevice = Omit<Device, 'statusName'>;
      const response = await api.get<BackendDevice>(`/api/devices/${id}`);
      return {
        ...response.data,
        statusName: DEVICE_STATUS_LABELS[response.data.status] || 'Unknown'
      };
    } catch (error) {
      console.error('Error fetching device by ID:', error);
      return null;
    }
  },

  // Create new device
  async createDevice(deviceData: DeviceCreateRequest): Promise<Device | null> {
    try {
      type BackendDevice = Omit<Device, 'statusName'>;
      const response = await api.post<BackendDevice>('/api/devices', deviceData);
      return {
        ...response.data,
        statusName: DEVICE_STATUS_LABELS[response.data.status] || 'Unknown'
      };
    } catch (error) {
      console.error('Error creating device:', error);
      return null;
    }
  },

  // Update device
  async updateDevice(id: number, deviceData: Partial<DeviceCreateRequest>): Promise<Device | null> {
    try {
      type BackendDevice = Omit<Device, 'statusName'>;
      const response = await api.put<BackendDevice>(`/api/devices/${id}`, deviceData);
      return {
        ...response.data,
        statusName: DEVICE_STATUS_LABELS[response.data.status] || 'Unknown'
      };
    } catch (error) {
      console.error('Error updating device:', error);
      return null;
    }
  },

  // Delete device
  async deleteDevice(id: number): Promise<boolean> {
    try {
      await api.delete(`/api/devices/${id}`);
      return true;
    } catch (error) {
      console.error('Error deleting device:', error);
      return false;
    }
  },

  // Get status distribution for charts
  async getStatusDistribution(): Promise<Record<string, number> | null> {
    try {
      const response = await api.get<Record<string, number>>('/api/devices/status-distribution');
      return response.data;
    } catch (error) {
      console.error('Error fetching status distribution:', error);
      return null;
    }
  }
};
