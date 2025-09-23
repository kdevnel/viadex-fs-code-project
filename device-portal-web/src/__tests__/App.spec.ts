import { describe, it, expect, vi } from 'vitest'

import { mount } from '@vue/test-utils'
import App from '../App.vue'
import { createTestingPinia } from '@pinia/testing';
import { useDevicesStore } from '../stores/useDevices';
import Devices from '../views/Devices.vue';
import { createRouter, createWebHistory } from 'vue-router';

const router = createRouter({
  history: createWebHistory(),
  routes: [],
});

describe('App', () => {
  it('mounts renders properly', () => {
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
    const pinia = createTestingPinia({ createSpy: vi.fn });
    const devicesStore = useDevicesStore(pinia);

    devicesStore.fetchDevices = vi.fn(); // Mock fetchDevices
    devicesStore.filters.status = ''; // Initialize status filter

    // Mocking statusDistribution as a computed property
    vi.spyOn(devicesStore, 'statusDistribution', 'get').mockReturnValue({
      Active: 5,
      Retired: 3,
      UnderRepair: 2,
    });

    const wrapper = mount(Devices, {
      global: {
        plugins: [pinia],
      },
    });

    // Simulate status filter change
    const statusDropdown = wrapper.find('[data-test="status-filter"]');
    console.log('Initial status filter:', devicesStore.filters.status);
    await statusDropdown.setValue('Active');
    console.log('Updated status filter:', devicesStore.filters.status);

    devicesStore.filters.status = 'Active'; // Explicitly set the status filter
    console.log('Explicitly set status filter:', devicesStore.filters.status);

    // Simulate change event
    await statusDropdown.trigger('change');
    console.log('After triggering change event, status filter:', devicesStore.filters.status);

    expect(devicesStore.filters.status).toBe('Active');
    expect(devicesStore.fetchDevices).toHaveBeenCalled();
  });
});
