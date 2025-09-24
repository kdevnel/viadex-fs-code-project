// TypeScript interfaces for Quote calculation functionality

export interface QuoteCalculation {
  deviceId: number;
  deviceName: string;
  deviceModel: string;
  customerName: string;
  durationMonths: number;
  supportTier: number;
  supportTierName: string;
  monthlyRate: number;
  supportRate: number;
  totalMonthlyCost: number;
  totalCost: number;
  validUntil: string;
}

export interface QuoteCalculateRequest {
  deviceId: number;
  customerName: string;
  durationMonths: number;
  supportTier: number;
}

export enum SupportTier {
  Basic = 1,
  Standard = 2,
  Premium = 3
}

export const SUPPORT_TIER_LABELS: Record<number, string> = {
  [SupportTier.Basic]: 'Basic',
  [SupportTier.Standard]: 'Standard',
  [SupportTier.Premium]: 'Premium'
};

export const SUPPORT_TIER_COLORS: Record<number, string> = {
  [SupportTier.Basic]: '#10b981',    // Green
  [SupportTier.Standard]: '#3b82f6', // Blue
  [SupportTier.Premium]: '#f59e0b'   // Orange
};

export const SUPPORT_TIER_DESCRIPTIONS: Record<number, string> = {
  [SupportTier.Basic]: 'Basic support (+0%)',
  [SupportTier.Standard]: 'Standard support (+20%)',
  [SupportTier.Premium]: 'Premium support (+50%)'
};

export const DURATION_OPTIONS = [
  { value: 6, label: '6 months' },
  { value: 12, label: '12 months' },
  { value: 18, label: '18 months' },
  { value: 24, label: '24 months' },
  { value: 36, label: '36 months' }
];
