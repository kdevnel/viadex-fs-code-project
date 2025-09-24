<template>
  <div class="chart-container">
    <div class="chart-header">
      <h3 class="chart-title">Shipment Status Distribution</h3>
      <p class="chart-subtitle">{{ totalShipments }} total shipments</p>
    </div>

    <div v-if="loading" class="chart-loading">
      <p>Loading chart data...</p>
    </div>

    <div v-else-if="error" class="chart-error">
      <p>{{ error }}</p>
    </div>

    <div v-else-if="hasData" class="chart-content">
      <Bar :data="chartData" :options="chartOptions" />

      <!-- Status summary -->
      <div class="chart-summary">
        <div
          v-for="item in summaryItems"
          :key="item.label"
          class="summary-item"
        >
          <div
            class="summary-color"
            :style="{ backgroundColor: item.color }"
          ></div>
          <div class="summary-content">
            <span class="summary-label">{{ item.label }}</span>
            <span class="summary-count">{{ item.count }}</span>
          </div>
        </div>
      </div>
    </div>

    <div v-else class="chart-empty">
      <p>No shipment data available</p>
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed } from 'vue';
import { Bar } from 'vue-chartjs';
import {
  Chart as ChartJS,
  CategoryScale,
  LinearScale,
  BarElement,
  Title,
  Tooltip,
  Legend
} from 'chart.js';
import type { ChartOptions } from 'chart.js';
import { useShipmentsStore } from '@/stores/useShipments';

// Register Chart.js components
ChartJS.register(CategoryScale, LinearScale, BarElement, Title, Tooltip, Legend);

const shipmentStore = useShipmentsStore();

// Status colors matching your application theme
const STATUS_COLORS = {
  processing: '#3b82f6',   // Blue - processing
  inTransit: '#f59e0b',    // Orange - in progress
  delivered: '#10b981',    // Green - success
  delayed: '#ef4444'       // Red - warning
} as const;

// Computed chart data
const chartData = computed(() => ({
  labels: ['Processing', 'In Transit', 'Delivered', 'Delayed'],
  datasets: [{
    label: 'Shipments',
    data: [
      shipmentStore.statusDistribution.processing || 0,
      shipmentStore.statusDistribution.inTransit || 0,
      shipmentStore.statusDistribution.delivered || 0,
      shipmentStore.statusDistribution.delayed || 0
    ],
    backgroundColor: [
      STATUS_COLORS.processing,
      STATUS_COLORS.inTransit,
      STATUS_COLORS.delivered,
      STATUS_COLORS.delayed
    ],
    borderColor: [
      STATUS_COLORS.processing,
      STATUS_COLORS.inTransit,
      STATUS_COLORS.delivered,
      STATUS_COLORS.delayed
    ],
    borderWidth: 2,
    borderRadius: 4,
    borderSkipped: false
  }]
}));

// Chart configuration
const chartOptions = computed<ChartOptions<'bar'>>(() => ({
  responsive: true,
  maintainAspectRatio: false,
  plugins: {
    legend: {
      display: false // Using custom summary
    },
    tooltip: {
      backgroundColor: 'rgba(0, 0, 0, 0.8)',
      titleColor: '#ffffff',
      bodyColor: '#ffffff',
      callbacks: {
        label: (context) => {
          const value = context.parsed.y;
          const total = totalShipments.value;
          const percentage = total > 0 ? Math.round((value / total) * 100) : 0;
          return `${value} shipments (${percentage}%)`;
        }
      }
    }
  },
  scales: {
    x: {
      grid: {
        display: false
      },
      ticks: {
        color: '#6b7280',
        font: {
          size: 12
        }
      }
    },
    y: {
      beginAtZero: true,
      grid: {
        color: '#f3f4f6'
      },
      ticks: {
        color: '#6b7280',
        font: {
          size: 12
        },
        stepSize: 1
      }
    }
  },
  animation: {
    duration: 1000,
    easing: 'easeOutQuart'
  }
}));

// Summary data
const summaryItems = computed(() => [
  {
    label: 'Processing',
    count: shipmentStore.statusDistribution.processing || 0,
    color: STATUS_COLORS.processing
  },
  {
    label: 'In Transit',
    count: shipmentStore.statusDistribution.inTransit || 0,
    color: STATUS_COLORS.inTransit
  },
  {
    label: 'Delivered',
    count: shipmentStore.statusDistribution.delivered || 0,
    color: STATUS_COLORS.delivered
  },
  {
    label: 'Delayed',
    count: shipmentStore.statusDistribution.delayed || 0,
    color: STATUS_COLORS.delayed
  }
]);

// Computed properties for state management
const loading = computed(() => shipmentStore.loading);
const error = computed(() => shipmentStore.error);
const totalShipments = computed(() => {
  const dist = shipmentStore.statusDistribution;
  return (dist.processing || 0) + (dist.inTransit || 0) + (dist.delivered || 0) + (dist.delayed || 0);
});
const hasData = computed(() => totalShipments.value > 0);
</script>

<style scoped>
.chart-container {
  background: white;
  border-radius: 8px;
  padding: 1.5rem;
  box-shadow: 0 1px 3px rgba(0, 0, 0, 0.1);
  border: 1px solid #e5e7eb;
}

.chart-header {
  margin-bottom: 1.5rem;
  text-align: center;
}

.chart-title {
  font-size: 1.25rem;
  font-weight: 600;
  color: #374151;
  margin: 0 0 0.5rem 0;
}

.chart-subtitle {
  font-size: 0.875rem;
  color: #6b7280;
  margin: 0;
}

.chart-content {
  display: flex;
  flex-direction: column;
  gap: 1.5rem;
}

.chart-content > div:first-child {
  width: 100%;
  height: 300px;
}

.chart-summary {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
  gap: 1rem;
}

.summary-item {
  display: flex;
  align-items: center;
  gap: 0.75rem;
  padding: 0.75rem;
  border-radius: 6px;
  background: #f8fafc;
  border: 1px solid #e5e7eb;
}

.summary-color {
  width: 12px;
  height: 12px;
  border-radius: 2px;
  flex-shrink: 0;
}

.summary-content {
  display: flex;
  flex-direction: column;
  gap: 0.25rem;
}

.summary-label {
  font-size: 0.875rem;
  color: #6b7280;
  font-weight: 500;
}

.summary-count {
  font-size: 1.125rem;
  font-weight: 600;
  color: #1f2937;
}

.chart-loading,
.chart-error,
.chart-empty {
  text-align: center;
  padding: 2rem;
  color: #6b7280;
}

.chart-error {
  color: #ef4444;
}

/* Responsive design */
@media (min-width: 768px) {
  .chart-summary {
    grid-template-columns: repeat(4, 1fr);
  }
}

@media (min-width: 1024px) {
  .chart-content {
    flex-direction: row;
    align-items: flex-start;
  }

  .chart-content > div:first-child {
    flex: 2;
    min-width: 400px;
  }

  .chart-summary {
    flex: 1;
    grid-template-columns: 1fr;
    min-width: 250px;
  }
}
</style>
