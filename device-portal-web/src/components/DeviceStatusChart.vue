<template>
  <div class="chart-container">
    <div class="chart-header">
      <h3 class="chart-title">Device Status Distribution</h3>
      <p class="chart-subtitle">{{ totalDevices }} total devices</p>
    </div>

    <div v-if="loading" class="chart-loading">
      <p>Loading chart data...</p>
    </div>

    <div v-else-if="error" class="chart-error">
      <p>{{ error }}</p>
    </div>

    <div v-else-if="hasData" class="chart-content">
      <Doughnut :data="chartData" :options="chartOptions" />

      <!-- Legend with counts -->
      <div class="chart-legend">
        <div
          v-for="item in legendItems"
          :key="item.label"
          class="legend-item"
        >
          <div
            class="legend-color"
            :style="{ backgroundColor: item.color }"
          ></div>
          <span class="legend-label">{{ item.label }}</span>
          <span class="legend-count">{{ item.count }}</span>
        </div>
      </div>
    </div>

    <div v-else class="chart-empty">
      <p>No device data available</p>
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed } from 'vue';
import { Doughnut } from 'vue-chartjs';
import {
  Chart as ChartJS,
  ArcElement,
  Tooltip,
  Legend
} from 'chart.js';
import type { ChartOptions } from 'chart.js';
import { useDevicesStore } from '@/stores/useDevices';

// Register Chart.js components
ChartJS.register(ArcElement, Tooltip, Legend);

const deviceStore = useDevicesStore();

// Status colors matching your application theme
const STATUS_COLORS = {
  active: '#10b981',     // Green - success
  retired: '#6b7280',    // Gray - neutral
  underRepair: '#f59e0b' // Orange - warning
} as const;

// Computed chart data
const chartData = computed(() => ({
  labels: ['Active', 'Retired', 'Under Repair'],
  datasets: [{
    data: [
      deviceStore.statusDistribution.active,
      deviceStore.statusDistribution.retired,
      deviceStore.statusDistribution.underRepair
    ],
    backgroundColor: [
      STATUS_COLORS.active,
      STATUS_COLORS.retired,
      STATUS_COLORS.underRepair
    ],
    borderWidth: 2,
    borderColor: '#ffffff',
    hoverBorderWidth: 3
  }]
}));

// Chart configuration
const chartOptions = computed<ChartOptions<'doughnut'>>(() => ({
  responsive: true,
  maintainAspectRatio: false,
  plugins: {
    legend: {
      display: false // Using custom legend
    },
    tooltip: {
      backgroundColor: 'rgba(0, 0, 0, 0.8)',
      titleColor: '#ffffff',
      bodyColor: '#ffffff',
      callbacks: {
        label: (context) => {
          const label = context.label || '';
          const value = context.parsed;
          const total = totalDevices.value;
          const percentage = total > 0 ? Math.round((value / total) * 100) : 0;
          return `${label}: ${value} devices (${percentage}%)`;
        }
      }
    }
  },
  cutout: '60%', // Doughnut hole size
  animation: {
    animateRotate: true,
    duration: 1000
  }
}));

// Legend data
const legendItems = computed(() => [
  {
    label: 'Active',
    count: deviceStore.statusDistribution.active,
    color: STATUS_COLORS.active
  },
  {
    label: 'Retired',
    count: deviceStore.statusDistribution.retired,
    color: STATUS_COLORS.retired
  },
  {
    label: 'Under Repair',
    count: deviceStore.statusDistribution.underRepair,
    color: STATUS_COLORS.underRepair
  }
]);

// Computed properties for state management
const loading = computed(() => deviceStore.loading);
const error = computed(() => deviceStore.error);
const totalDevices = computed(() => deviceStore.totalDevicesFromStatus);
const hasData = computed(() => totalDevices.value > 0);
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
  align-items: center;
  gap: 1.5rem;
}

.chart-content > div:first-child {
  width: 100%;
  height: 300px;
  max-width: 400px;
}

.chart-legend {
  display: flex;
  flex-direction: column;
  gap: 0.75rem;
  width: 100%;
  max-width: 300px;
}

.legend-item {
  display: flex;
  align-items: center;
  gap: 0.75rem;
  padding: 0.5rem;
  border-radius: 6px;
  background: #f8fafc;
}

.legend-color {
  width: 16px;
  height: 16px;
  border-radius: 50%;
  flex-shrink: 0;
}

.legend-label {
  flex: 1;
  font-weight: 500;
  color: #374151;
}

.legend-count {
  font-weight: 600;
  color: #1f2937;
  background: white;
  padding: 0.25rem 0.5rem;
  border-radius: 4px;
  font-size: 0.875rem;
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
  .chart-content {
    flex-direction: row;
    align-items: flex-start;
    justify-content: space-between;
  }

  .chart-content > div:first-child {
    width: 300px;
    height: 300px;
  }

  .chart-legend {
    flex-shrink: 0;
    width: 200px;
  }
}
</style>
