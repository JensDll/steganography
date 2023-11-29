import { type RouteRecordRaw, createRouter, createWebHistory } from 'vue-router'

import CodecView from '~/features/codec/routes/CodecView.vue'
import DecodeView from '~/features/codec/routes/DecodeView.vue'
import EncodeView from '~/features/codec/routes/EncodeView.vue'

const routes: RouteRecordRaw[] = [
  {
    name: 'home',
    path: '/',
    redirect: { name: 'codec' },
  },
  {
    name: 'codec',
    path: '/codec',
    redirect: { name: 'encode' },
    component: CodecView,
    children: [
      {
        name: 'encode',
        path: 'encode',
        alias: '',
        component: EncodeView,
        meta: {
          title: 'Encode',
        },
      },
      {
        name: 'decode',
        path: 'decode',
        component: DecodeView,
        meta: {
          title: 'Decode',
        },
      },
    ],
  },
]

export const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes,
})
