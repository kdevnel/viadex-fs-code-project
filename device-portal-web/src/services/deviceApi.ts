// Device-specific API service using native fetch
import apiClient, { type ApiResponse } from './api';

// Device types matching your backend DTOs
export interface Device {
  id: number;
  name: string;
  model: string;
  monthlyPrice: number;
  purchaseDate: string;
  status?: 'Active' | 'Retired' | 'UnderRepair';
}

export interface DeviceCreateRequest {
  name: string;
  model: string;
  monthlyPrice: number;
}

// Backend response structure (actual runtime response - lowercase properties)
export interface BackendDevicePagedResponse {
  total: number;
  items: Device[];
}

// Frontend expected structure
export interface DevicePagedResult {
  total: number;
  items: Device[];
  isSuccess: boolean;
  errorMessage?: string;
}

export interface DeviceFilters {
  page?: number;
  pageSize?: number;
  status?: string;
  searchTerm?: string;
}

// Device API service
export const deviceApi = {
    // Get paginated devices with filtering
  async getDevices(filters: DeviceFilters = {}): Promise<DevicePagedResult> {
    try {
      const params = {
        page: String(filters.page || 1),
        pageSize: String(filters.pageSize || 20),
        ...(filters.status && { status: filters.status }),
        ...(filters.searchTerm && { searchTerm: filters.searchTerm }),
      };

      const response: ApiResponse<BackendDevicePagedResponse> = await apiClient.get('/api/devices', params);

      // The API returns lowercase properties: { total: number, items: Device[] }
      return {
        isSuccess: true,
        total: response.data.total,
        items: response.data.items,
      };
    } catch (error) {
      return {
        isSuccess: false,
        errorMessage: error instanceof Error ? error.message : 'Failed to fetch devices',
        total: 0,
        items: [],
      };
    }
  },

  // Get single device by ID
  async getDevice(id: number): Promise<{ device?: Device; error?: string }> {
    try {
      const response: ApiResponse<Device> = await apiClient.get(`/api/devices/${id}`);
      return { device: response.data };
    } catch (error) {
      return {
        error: error instanceof Error ? error.message : 'Failed to fetch device'
      };
    }
  },

  // Create new device
  async createDevice(device: DeviceCreateRequest): Promise<{ device?: Device; error?: string }> {
    try {
      const response: ApiResponse<Device> = await apiClient.post('/api/devices', device);
      return { device: response.data };
    } catch (error) {
      return {
        error: error instanceof Error ? error.message : 'Failed to create device'
      };
    }
  },

  // Update device
  async updateDevice(id: number, device: Partial<DeviceCreateRequest>): Promise<{ device?: Device; error?: string }> {
    try {
      const response: ApiResponse<Device> = await apiClient.put(`/api/devices/${id}`, device);
      return { device: response.data };
    } catch (error) {
      return {
        error: error instanceof Error ? error.message : 'Failed to update device'
      };
    }
  },

  // Delete device
  async deleteDevice(id: number): Promise<{ success: boolean; error?: string }> {
    try {
      await apiClient.delete(`/api/devices/${id}`);
      return { success: true };
    } catch (error) {
      return {
        success: false,
        error: error instanceof Error ? error.message : 'Failed to delete device'
      };
    }
  },

  // Get device status distribution for charts
  async getStatusDistribution(): Promise<{ data?: Record<string, number>; error?: string }> {
    try {
      const response: ApiResponse<Record<string, number>> = await apiClient.get('/api/devices/status-distribution');
      return { data: response.data };
    } catch (error) {
      return {
        error: error instanceof Error ? error.message : 'Failed to fetch status distribution'
      };
    }
  },
};
