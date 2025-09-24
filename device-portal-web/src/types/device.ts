// Typescript interfaces for device-related data structures

export interface Device {
  id: number;
  name: string;
  model: string;
  monthlyPrice: number;
  purchaseDate: string;
  status: number;
  statusName: string;
}

export interface DeviceCreateRequest {
  name: string;
  model: string;
  monthlyPrice: number;
}

// Backend response structure
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

export interface DeviceStatusDistribution {
  active: number;
  retired: number;
  underRepair: number;
}

export interface DeviceFilters {
  page: number;
  pageSize: number;
  status?: number;
  searchTerm: string;
}

export enum DeviceStatus {
  Active = 1,
  Retired = 2,
  UnderRepair = 3
}

export const DEVICE_STATUS_LABELS: Record<number, string> = {
  [DeviceStatus.Active]: 'Active',
  [DeviceStatus.Retired]: 'Retired',
  [DeviceStatus.UnderRepair]: 'Under Repair'
};

export const DEVICE_STATUS_COLORS: Record<number, string> = {
  [DeviceStatus.Active]: '#10b981',
  [DeviceStatus.Retired]: '#6b7280',
  [DeviceStatus.UnderRepair]: '#f59e0b'
};
