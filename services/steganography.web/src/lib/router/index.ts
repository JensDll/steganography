import { type RouteRecordRaw, createRouter, createWebHistory } from 'vue-router'

import AboutViewVue from '~/features/about/routes/AboutView.vue'
import CodecView from '~/features/codec/routes/CodecView.vue'
import DecodeView from '~/features/codec/routes/DecodeView.vue'
import EncodeView from '~/features/codec/routes/EncodeView.vue'
import HomeView from '~/features/home/routes/HomeView.vue'

const routes: RouteRecordRaw[] = [
  {
    name: 'home',
    path: '/',
    component: HomeView,
    meta: {
      title: 'Steganography'
    }
  },
  {
    name: 'about',
    path: '/about',
    component: AboutViewVue,
    meta: {
      title: 'About'
    }
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
          title: 'Encode'
        }
      },
      {
        name: 'decode',
        path: 'decode',
        component: DecodeView,
        meta: {
          title: 'Decode'
        }
      }
    ]
  },
  {
    path: '/:pathMatch(.*)*',
    name: 'notFound',
    component: () => import('~/features/not_found/routes/NotFoundView.vue'),
    meta: { title: 'Not Found' }
  }
]

export const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes
})

const titleTag = document.head.querySelector('title')!

router.beforeEach(to => {
  titleTag.text = to.meta.title

  if (to.name === 'encode') {
    document.documentElement.classList.add('encode')
    document.documentElement.classList.remove('decode')
  } else if (to.name === 'decode') {
    document.documentElement.classList.remove('encode')
    document.documentElement.classList.add('decode')
  }
})

declare module 'vue-router' {
  interface RouteMeta {
    title: string
  }
}
