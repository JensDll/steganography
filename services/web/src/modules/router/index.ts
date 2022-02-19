import { createRouter, createWebHistory, type RouteRecordRaw } from 'vue-router'

import HomePage from '~/pages/home/Index.vue'

import CodecPage from '~/pages/codec/Index.vue'
import EncodeForm from '~/pages/codec/EncodeForm.vue'
import DecodeForm from '~/pages/codec/DecodeForm.vue'

const routes: RouteRecordRaw[] = [
  {
    name: 'home',
    path: '/',
    component: HomePage
  },
  {
    name: 'codec',
    path: '/codec',
    component: CodecPage,
    children: [
      {
        name: 'encode',
        path: 'encode',
        component: EncodeForm,
        meta: {
          title: 'Encode'
        }
      },
      {
        name: 'decode',
        path: 'decode',
        component: DecodeForm,
        meta: {
          title: 'Decode'
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
    title?: string
  }
}
