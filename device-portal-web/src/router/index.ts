import { createRouter, createWebHistory } from 'vue-router'
import Devices from '@/views/Devices.vue'

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
    }
  ],
})

export default router
