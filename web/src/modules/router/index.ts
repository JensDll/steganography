import { createRouter, createWebHistory, RouteRecordRaw } from 'vue-router'

import LandingPage from '~/views/landing/LandingPage.vue'
import EncodeForm from '~/views/landing/EncodeForm.vue'
import DecodeForm from '~/views/landing/DecodeForm.vue'

const routes: RouteRecordRaw[] = [
  {
    path: '/',
    redirect: { name: 'encode' }
  },
  {
    path: '/home',
    redirect: { name: 'encode' }
  },
  {
    name: 'landing',
    path: '/',
    component: LandingPage,
    children: [
      {
        name: 'encode',
        path: '/encode',
        components: {
          default: EncodeForm
        },
        meta: {
          transition: 'slide-left'
        }
      },
      {
        name: 'decode',
        path: '/decode',
        components: {
          default: DecodeForm
        },
        meta: {
          transition: 'slide-right'
        }
      }
    ]
  }
]

export const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes
})

declare module 'vue-router' {
  interface RouteMeta {
    transition?: 'slide-left' | 'slide-right'
  }
}
