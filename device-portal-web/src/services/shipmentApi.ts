// Shipment API service using the native fetch client
import api from './api';
import type {
  Shipment,
  ShipmentCreateRequest,
  ShipmentUpdateStatusRequest,
  ShipmentsPagedResponse,
  ShipmentStatusDistribution
} from '@/types/shipment';

export const shipmentApi = {
  // Get all shipments with optional filtering and pagination
  async getShipments(params?: {
    page?: number;
    pageSize?: number;
    status?: number;
  }): Promise<ShipmentsPagedResponse> {
    const queryParams = new URLSearchParams();

    if (params?.page) queryParams.append('page', params.page.toString());
    if (params?.pageSize) queryParams.append('pageSize', params.pageSize.toString());
    if (params?.status) queryParams.append('status', params.status.toString());

    const endpoint = `/api/shipments${queryParams.toString() ? `?${queryParams.toString()}` : ''}`;

    try {
      const response = await api.get<ShipmentsPagedResponse>(endpoint);
      return response.data;
    } catch (error) {
      console.error('Error fetching shipments:', error);
      return {
        isSuccess: false,
        total: 0,
        items: [],
        errorMessage: error instanceof Error ? error.message : 'Failed to fetch shipments'
      };
    }
  },

  // Get shipment by ID
  async getShipmentById(id: number): Promise<Shipment | null> {
    try {
      const response = await api.get<Shipment>(`/api/shipments/${id}`);
      return response.data;
    } catch (error) {
      console.error('Error fetching shipment by ID:', error);
      return null;
    }
  },

  // Track shipment by tracking number (core feature)
  async trackShipment(trackingNumber: string): Promise<Shipment | null> {
    try {
      const response = await api.get<Shipment>(`/api/shipments/track/${trackingNumber}`);
      return response.data;
    } catch (error) {
      console.error('Error tracking shipment:', error);
      return null;
    }
  },

  // Create new shipment
  async createShipment(shipmentData: ShipmentCreateRequest): Promise<Shipment | null> {
    try {
      const response = await api.post<Shipment>('/api/shipments', shipmentData);
      return response.data;
    } catch (error) {
      console.error('Error creating shipment:', error);
      return null;
    }
  },

  // Update shipment status
  async updateShipmentStatus(id: number, statusData: ShipmentUpdateStatusRequest): Promise<Shipment | null> {
    try {
      const response = await api.patch<Shipment>(`/api/shipments/${id}/status`, statusData);
      return response.data;
    } catch (error) {
      console.error('Error updating shipment status:', error);
      return null;
    }
  },

  // Get status distribution for charts
  async getStatusDistribution(): Promise<ShipmentStatusDistribution | null> {
    try {
      const response = await api.get<ShipmentStatusDistribution>('/api/shipments/status-distribution');
      return response.data;
    } catch (error) {
      console.error('Error fetching status distribution:', error);
      return null;
    }
  }
};
