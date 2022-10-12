import { type RouteRecordRaw, createRouter, createWebHistory } from 'vue-router'

import AboutIndex from '~/pages/about/Index.vue'
import DecodeForm from '~/pages/codec/DecodeForm.vue'
import EncodeForm from '~/pages/codec/EncodeForm.vue'
import CodecIndex from '~/pages/codec/Index.vue'
import HomeIndex from '~/pages/home/Index.vue'
import NotFoundIndex from '~/pages/not_found/Index.vue'

const routes: RouteRecordRaw[] = [
  {
    name: 'home',
    path: '/',
    component: HomeIndex,
    meta: {
      title: 'Image Data Hiding'
    }
  },
  {
    name: 'about',
    path: '/about',
    component: AboutIndex,
    meta: {
      title: 'About',
      description:
        'Online image Steganography. Here you will find some notes about the project and how it works.'
    }
  },
  {
    name: 'codec',
    path: '/codec',
    redirect: { name: 'encode' },
    component: CodecIndex,
    children: [
      {
        name: 'encode',
        path: 'encode',
        alias: '',
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
  },
  {
    path: '/:pathMatch(.*)*',
    name: 'notFound',
    component: NotFoundIndex,
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
