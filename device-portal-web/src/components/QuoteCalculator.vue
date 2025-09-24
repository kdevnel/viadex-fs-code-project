<template>
  <div class="quote-calculator">
    <div class="calculator-header">
      <h2>Quote Calculator</h2>
      <p>Calculate leasing quotes for devices with support tiers</p>
    </div>

    <!-- Quote Calculation Form -->
    <form @submit.prevent="calculateQuote" class="quote-form">
      <div class="form-row">
        <div class="form-group">
          <label for="device" class="form-label">Device *</label>
          <select
            id="device"
            v-model="form.deviceId"
            :disabled="devicesLoading"
            class="form-input"
            required
          >
            <option value="">{{ devicesLoading ? 'Loading devices...' : 'Select a device' }}</option>
            <option
              v-for="device in availableDevices"
              :key="device.id"
              :value="device.id"
            >
              {{ device.name }} - {{ device.model }} (£{{ device.monthlyPrice }}/month)
            </option>
          </select>
        </div>

        <div class="form-group">
          <label for="customer" class="form-label">Customer Name *</label>
          <input
            id="customer"
            v-model="form.customerName"
            type="text"
            placeholder="Enter customer name"
            class="form-input"
            required
          />
        </div>
      </div>

      <div class="form-row">
        <div class="form-group">
          <label for="duration" class="form-label">Lease Duration *</label>
          <select
            id="duration"
            v-model="form.durationMonths"
            class="form-input"
            required
          >
            <option value="">Select duration</option>
            <option
              v-for="option in durationOptions"
              :key="option.value"
              :value="option.value"
            >
              {{ option.label }}
            </option>
          </select>
        </div>

        <div class="form-group">
          <label for="support" class="form-label">Support Tier *</label>
          <select
            id="support"
            v-model="form.supportTier"
            class="form-input"
            required
          >
            <option value="">Select support tier</option>
            <option
              v-for="tier in supportTierOptions"
              :key="tier.value"
              :value="tier.value"
            >
              {{ tier.label }} - {{ tier.description }}
            </option>
          </select>
        </div>
      </div>

      <div class="form-actions">
        <button
          type="submit"
          :disabled="quoteStore.calculationLoading || !isFormValid"
          class="btn btn-primary"
        >
          {{ quoteStore.calculationLoading ? 'Calculating...' : 'Calculate Quote' }}
        </button>
        <button
          type="button"
          @click="clearForm"
          class="btn btn-secondary"
        >
          Clear Form
        </button>
      </div>
    </form>

    <!-- Calculation Error -->
    <div v-if="quoteStore.calculationError" class="error-message">
      <p>{{ quoteStore.calculationError }}</p>
    </div>

    <!-- Quote Result -->
    <div v-if="quoteStore.calculatedQuote" class="quote-result">
      <div class="result-header">
        <h3>Quote Calculation Result</h3>
        <button @click="quoteStore.clearCalculatedQuote()" class="close-btn">×</button>
      </div>

      <div class="result-content">
        <div class="result-section">
          <h4>Customer & Device Details</h4>
          <div class="detail-grid">
            <div class="detail-item">
              <span class="detail-label">Customer:</span>
              <span class="detail-value">{{ quoteStore.calculatedQuote.customerName }}</span>
            </div>
            <div class="detail-item">
              <span class="detail-label">Device:</span>
              <span class="detail-value">{{ quoteStore.calculatedQuote.deviceName }} - {{ quoteStore.calculatedQuote.deviceModel }}</span>
            </div>
            <div class="detail-item">
              <span class="detail-label">Duration:</span>
              <span class="detail-value">{{ quoteStore.calculatedQuote.durationMonths }} months</span>
            </div>
            <div class="detail-item">
              <span class="detail-label">Support Tier:</span>
              <span class="detail-value">{{ quoteStore.calculatedQuote.supportTierName }}</span>
            </div>
          </div>
        </div>

        <div class="result-section">
          <h4>Pricing Breakdown</h4>
          <div class="pricing-grid">
            <div class="pricing-item">
              <span class="pricing-label">Base Monthly Rate:</span>
              <span class="pricing-value">£{{ quoteStore.calculatedQuote.monthlyRate.toFixed(2) }}</span>
            </div>
            <div class="pricing-item">
              <span class="pricing-label">Support Rate:</span>
              <span class="pricing-value">£{{ quoteStore.calculatedQuote.supportRate.toFixed(2) }}</span>
            </div>
            <div class="pricing-item total">
              <span class="pricing-label">Total Monthly Cost:</span>
              <span class="pricing-value">£{{ quoteStore.calculatedQuote.totalMonthlyCost.toFixed(2) }}</span>
            </div>
            <div class="pricing-item final-total">
              <span class="pricing-label">Total Lease Cost:</span>
              <span class="pricing-value">£{{ quoteStore.calculatedQuote.totalCost.toFixed(2) }}</span>
            </div>
          </div>
        </div>

        <div class="result-section">
          <h4>Quote Validity</h4>
          <p class="validity-info">
            This quote is valid until: {{ formatDate(quoteStore.calculatedQuote.validUntil) }}
          </p>
        </div>
      </div>

      <div class="result-actions">
        <button
          @click="quoteStore.clearCalculatedQuote()"
          class="btn btn-primary"
        >
          Calculate Another
        </button>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { reactive, computed, onMounted } from 'vue';
import { useQuoteCalculatorStore } from '@/stores/useQuotes';
import { useDevicesStore } from '@/stores/useDevices';
import type { QuoteCalculateRequest } from '@/types/quote';
import { DURATION_OPTIONS } from '@/types/quote';

// Store initialization
const quoteStore = useQuoteCalculatorStore();
const devicesStore = useDevicesStore();

// Form state
const form = reactive<QuoteCalculateRequest>({
  deviceId: 0,
  customerName: '',
  durationMonths: 0,
  supportTier: 0
});

// Configuration options
const durationOptions = DURATION_OPTIONS;

const supportTierOptions = [
  { value: 1, label: 'Basic', description: '+0% support' },
  { value: 2, label: 'Standard', description: '+20% support' },
  { value: 3, label: 'Premium', description: '+50% support' }
];

// Computed properties
const devicesLoading = computed(() => devicesStore.loading);

const availableDevices = computed(() =>
  devicesStore.devices.filter(device => device.status === 1) // Only active devices
);

const isFormValid = computed(() =>
  form.deviceId > 0 &&
  form.customerName.trim() !== '' &&
  form.durationMonths > 0 &&
  form.supportTier > 0
);

// Methods
const calculateQuote = async () => {
  if (!isFormValid.value) return;

  await quoteStore.calculateQuote({
    deviceId: form.deviceId,
    customerName: form.customerName.trim(),
    durationMonths: form.durationMonths,
    supportTier: form.supportTier
  });
};

const clearForm = () => {
  form.deviceId = 0;
  form.customerName = '';
  form.durationMonths = 0;
  form.supportTier = 0;
  quoteStore.clearCalculatedQuote();
};

const formatDate = (dateString: string) => {
  return new Date(dateString).toLocaleDateString('en-GB');
};

// Initialize on mount
onMounted(async () => {
  // Load devices if not already loaded
  if (!devicesStore.hasDevices) {
    await devicesStore.fetchDevices();
  }
});
</script>

<style scoped>
.quote-calculator {
  max-width: 800px;
  margin: 0 auto;
  padding: 2rem;
}

.calculator-header {
  text-align: center;
  margin-bottom: 2rem;
}

.calculator-header h2 {
  color: #1f2937;
  margin-bottom: 0.5rem;
}

.calculator-header p {
  color: #6b7280;
}

.quote-form {
  background: white;
  padding: 2rem;
  border-radius: 8px;
  box-shadow: 0 1px 3px rgba(0, 0, 0, 0.1);
  margin-bottom: 2rem;
}

.form-row {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 1.5rem;
  margin-bottom: 1.5rem;
}

.form-group {
  display: flex;
  flex-direction: column;
}

.form-label {
  font-weight: 600;
  color: #374151;
  margin-bottom: 0.5rem;
}

.form-input {
  padding: 0.75rem;
  border: 1px solid #d1d5db;
  border-radius: 6px;
  font-size: 1rem;
  transition: border-color 0.2s;
}

.form-input:focus {
  outline: none;
  border-color: #3b82f6;
  box-shadow: 0 0 0 3px rgba(59, 130, 246, 0.1);
}

.form-actions {
  display: flex;
  gap: 1rem;
  justify-content: center;
  margin-top: 2rem;
}

.quote-result {
  background: white;
  border-radius: 8px;
  box-shadow: 0 1px 3px rgba(0, 0, 0, 0.1);
  overflow: hidden;
}

.result-header {
  background: #f8fafc;
  padding: 1.5rem;
  border-bottom: 1px solid #e5e7eb;
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.result-header h3 {
  margin: 0;
  color: #1f2937;
}

.close-btn {
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
  border-radius: 4px;
  transition: background-color 0.2s;
}

.close-btn:hover {
  background: #e5e7eb;
}

.result-content {
  padding: 2rem;
}

.result-section {
  margin-bottom: 2rem;
}

.result-section:last-child {
  margin-bottom: 0;
}

.result-section h4 {
  color: #1f2937;
  margin-bottom: 1rem;
  font-size: 1.1rem;
}

.detail-grid,
.pricing-grid {
  display: grid;
  gap: 0.75rem;
}

.detail-item,
.pricing-item {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 0.5rem 0;
  border-bottom: 1px solid #f3f4f6;
}

.detail-label,
.pricing-label {
  font-weight: 500;
  color: #6b7280;
}

.detail-value,
.pricing-value {
  color: #1f2937;
  font-weight: 600;
}

.pricing-item.total {
  background: #f8fafc;
  padding: 0.75rem;
  border-radius: 6px;
  border: none;
  margin-top: 0.5rem;
}

.pricing-item.final-total {
  background: #dbeafe;
  border: 2px solid #3b82f6;
  font-size: 1.1rem;
}

.pricing-item.final-total .pricing-value {
  color: #1e40af;
  font-size: 1.2rem;
}

.validity-info {
  background: #fef3c7;
  padding: 1rem;
  border-radius: 6px;
  color: #92400e;
  margin: 0;
}

.result-actions {
  padding: 1.5rem 2rem;
  background: #f8fafc;
  border-top: 1px solid #e5e7eb;
  display: flex;
  gap: 1rem;
  justify-content: center;
}

.error-message {
  background: #fef2f2;
  border: 1px solid #fecaca;
  color: #dc2626;
  padding: 1rem;
  border-radius: 6px;
  margin-bottom: 1rem;
}

@media (max-width: 768px) {
  .quote-calculator {
    padding: 1rem;
  }

  .form-row {
    grid-template-columns: 1fr;
    gap: 1rem;
  }

  .form-actions,
  .result-actions {
    flex-direction: column;
  }

  .result-header {
    padding: 1rem;
  }

  .result-content {
    padding: 1rem;
  }
}
</style>
