import { describe, it, expect, vi } from 'vitest'

import { mount } from '@vue/test-utils'
import App from '../App.vue'
import { createTestingPinia } from '@pinia/testing';
import { useDevicesStore } from '../stores/useDevices';
import Devices from '../views/Devices.vue';
import { createRouter, createWebHistory } from 'vue-router';

// Mock the API module to prevent actual network calls
vi.mock('../services/deviceApi', () => ({
  deviceApi: {
    getDevices: vi.fn().mockResolvedValue({
      isSuccess: true,
      items: [],
      total: 0
    }),
    getStatusDistribution: vi.fn().mockResolvedValue({
      Active: 5,
      Retired: 3,
      UnderRepair: 2
    })
  }
}));

const router = createRouter({
  history: createWebHistory(),
  routes: [
    { path: '/', component: Devices }
  ],
});

describe('App', () => {
  it('renders properly', () => {
    const wrapper = mount(App, {
      global: {
        plugins: [router],
      },
    })
    expect(wrapper.text()).toContain('Viadex Device Portal');
  })
})

describe('Devices View', () => {
  it('updates device list when status filter changes', async () => {
    const pinia = createTestingPinia({
      createSpy: vi.fn,
      stubActions: false
    });

    const devicesStore = useDevicesStore(pinia);

    // Mock the store methods
    devicesStore.setStatusFilter = vi.fn();

    // Mock the computed properties and reactive refs directly
    devicesStore.devices = [];
    devicesStore.totalDevices = 0;
    devicesStore.loading = false;
    devicesStore.error = null;
    devicesStore.filters = {
      page: 1,
      pageSize: 5,
      status: undefined,
      searchTerm: ''
    };

    // Mock the status distribution
    Object.defineProperty(devicesStore, 'statusDistribution', {
      get: () => ({ active: 5, retired: 3, underRepair: 2 }),
      configurable: true
    });

    Object.defineProperty(devicesStore, 'totalDevicesFromStatus', {
      get: () => 10,
      configurable: true
    });

    Object.defineProperty(devicesStore, 'activeFiltersCount', {
      get: () => 0,
      configurable: true
    });

    Object.defineProperty(devicesStore, 'hasDevices', {
      get: () => false,
      configurable: true
    });

    const wrapper = mount(Devices, {
      global: {
        plugins: [pinia, router],
        stubs: {
          DeviceStatusChart: true
        }
      },
    });

    // Find the status filter dropdown by class name
    const statusDropdown = wrapper.find('.status-filter');
    expect(statusDropdown.exists()).toBe(true);

    // Set value to numeric status (1 = Active) to match the actual implementation
    await statusDropdown.setValue('1');

    // Trigger the change event
    await statusDropdown.trigger('change');

    // Verify that setStatusFilter was called with numeric value
    expect(devicesStore.setStatusFilter).toHaveBeenCalledWith(1);
  });

  it('displays status distribution in overview cards', async () => {
    const pinia = createTestingPinia({
      createSpy: vi.fn,
      stubActions: false
    });

    const devicesStore = useDevicesStore(pinia);

    // Mock store properties
    devicesStore.devices = [];
    devicesStore.totalDevices = 10;
    devicesStore.loading = false;
    devicesStore.error = null;

    // Mock computed properties
    Object.defineProperty(devicesStore, 'statusDistribution', {
      get: () => ({ active: 5, retired: 3, underRepair: 2 }),
      configurable: true
    });

    Object.defineProperty(devicesStore, 'totalDevicesFromStatus', {
      get: () => 10,
      configurable: true
    });

    Object.defineProperty(devicesStore, 'activeFiltersCount', {
      get: () => 0,
      configurable: true
    });

    Object.defineProperty(devicesStore, 'hasDevices', {
      get: () => false,
      configurable: true
    });

    const wrapper = mount(Devices, {
      global: {
        plugins: [pinia, router],
        stubs: {
          DeviceStatusChart: true
        }
      },
    });

    // Check that status cards are rendered with correct values
    const statCards = wrapper.findAll('.stat-card');
    expect(statCards.length).toBeGreaterThan(0);

    // Find the Active status card and verify its value
    const activeCard = statCards.find(card => card.text().includes('Active'));
    expect(activeCard).toBeDefined();
    expect(activeCard?.text()).toContain('5');
  });

  it('clears filters when clear button is clicked', async () => {
    const pinia = createTestingPinia({
      createSpy: vi.fn,
      stubActions: false
    });

    const devicesStore = useDevicesStore(pinia);
    devicesStore.clearFilters = vi.fn();

    // Mock store properties
    devicesStore.devices = [];
    devicesStore.loading = false;
    devicesStore.error = null;
    devicesStore.filters = {
      page: 1,
      pageSize: 5,
      status: 1,
      searchTerm: ''
    };

    // Mock computed properties with active filters
    Object.defineProperty(devicesStore, 'activeFiltersCount', {
      get: () => 1,
      configurable: true
    });

    Object.defineProperty(devicesStore, 'statusDistribution', {
      get: () => ({ active: 5, retired: 3, underRepair: 2 }),
      configurable: true
    });

    Object.defineProperty(devicesStore, 'totalDevicesFromStatus', {
      get: () => 10,
      configurable: true
    });

    Object.defineProperty(devicesStore, 'hasDevices', {
      get: () => false,
      configurable: true
    });

    const wrapper = mount(Devices, {
      global: {
        plugins: [pinia, router],
        stubs: {
          DeviceStatusChart: true
        }
      },
    });

    // Find and click the clear filters button
    const clearButton = wrapper.find('.clear-filters');
    expect(clearButton.exists()).toBe(true);

    await clearButton.trigger('click');

    expect(devicesStore.clearFilters).toHaveBeenCalled();
  });
});
