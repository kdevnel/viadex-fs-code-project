// TypeScript interfaces for Shipment tracking functionality

export interface Shipment {
  id: number;
  trackingNumber: string;
  customerName: string;
  status: number;
  statusName: string;
  estimatedDelivery: string;
  actualDelivery?: string | null;
  destination: string;
  createdAt: string;
}

export interface ShipmentCreateRequest {
  trackingNumber: string;
  customerName: string;
  estimatedDelivery: string;
  destination: string;
}

export interface ShipmentUpdateStatusRequest {
  status: number;
  actualDelivery?: string | null;
}

export interface ShipmentsPagedResponse {
  isSuccess: boolean;
  total: number;
  items: Shipment[];
  errorMessage?: string;
}

export interface ShipmentStatusDistribution {
  processing: number;
  inTransit: number;
  delivered: number;
  delayed: number;
}

export enum ShipmentStatus {
  Processing = 1,
  InTransit = 2,
  Delivered = 3,
  Delayed = 4
}

export const SHIPMENT_STATUS_LABELS: Record<number, string> = {
  [ShipmentStatus.Processing]: 'Processing',
  [ShipmentStatus.InTransit]: 'In Transit',
  [ShipmentStatus.Delivered]: 'Delivered',
  [ShipmentStatus.Delayed]: 'Delayed'
};

export const SHIPMENT_STATUS_COLORS: Record<number, string> = {
  [ShipmentStatus.Processing]: '#3b82f6',
  [ShipmentStatus.InTransit]: '#f59e0b',
  [ShipmentStatus.Delivered]: '#10b981',
  [ShipmentStatus.Delayed]: '#ef4444'
};
