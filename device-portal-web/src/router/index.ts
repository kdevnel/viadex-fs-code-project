import { createRouter, createWebHistory } from 'vue-router'
import Devices from '@/views/Devices.vue'
import Shipments from '@/views/Shipments.vue'
import QuoteCalculator from '@/views/QuoteCalculator.vue'

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
    {
      path: '/',
      name: 'home',
      redirect: '/devices'
    },
    {
      path: '/devices',
      name: 'devices',
      component: Devices
    },
    {
      path: '/shipments',
      name: 'shipments',
      component: Shipments
    },
    {
      path: '/quotes',
      name: 'quotes',
      component: QuoteCalculator
    }
  ],
})

export default router
