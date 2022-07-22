import { createRouter, createWebHistory, type RouteRecordRaw } from 'vue-router'

import NotFoundIndex from '~/pages/not_found/Index.vue'

import HomeIndex from '~/pages/home/Index.vue'

import AboutIndex from '~/pages/about/Index.vue'

import CodecIndex from '~/pages/codec/Index.vue'
import EncodeForm from '~/pages/codec/EncodeForm.vue'
import DecodeForm from '~/pages/codec/DecodeForm.vue'

const mainDescription =
  'Online image Steganography. Hide text or binary data in images using this web-based tool. The data is first encrypted and then randomly embedded using a least significant bit substitution technique.'

const routes: RouteRecordRaw[] = [
  {
    name: 'home',
    path: '/',
    component: HomeIndex,
    meta: {
      title: 'Image Data Hiding',
      description: mainDescription
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
          title: 'Encode',
          description: mainDescription
        }
      },
      {
        name: 'decode',
        path: 'decode',
        component: DecodeForm,
        meta: {
          title: 'Decode',
          description: mainDescription
        }
      }
    ]
  },
  { path: '/:pathMatch(.*)*', name: 'notFound', component: NotFoundIndex }
]

export const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes
})

const titleTag = document.head.querySelector('title')!
const metaTitleTags = document.head.querySelectorAll('meta[property*="title"]')
const metaDescriptionTags = document.head.querySelectorAll(
  'meta[name="description"], meta[property*="description"]'
)
const metaUrlTags = document.head.querySelectorAll('meta[property*="url"]')

router.beforeEach(to => {
  titleTag.text = to.meta.title

  metaTitleTags.forEach(tag => {
    tag.setAttribute('content', to.meta.title)
  })

  metaDescriptionTags.forEach(tag => {
    tag.setAttribute('content', to.meta.description)
  })

  metaUrlTags.forEach(function (this: string, tag) {
    tag.setAttribute('content', this)
  }, `${APP_CONFIG.THIS_URI}${to.fullPath}`)

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
    description: string
  }
}
