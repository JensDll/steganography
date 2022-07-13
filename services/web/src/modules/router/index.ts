import { createRouter, createWebHistory, type RouteRecordRaw } from 'vue-router'

import NotFoundIndex from '~/pages/not_found/Index.vue'

import HomeIndex from '~/pages/home/Index.vue'

import AboutIndex from '~/pages/about/Index.vue'

import CodecIndex from '~/pages/codec/Index.vue'
import EncodeForm from '~/pages/codec/EncodeForm.vue'
import DecodeForm from '~/pages/codec/DecodeForm.vue'

const routes: RouteRecordRaw[] = [
  {
    name: 'home',
    path: '/',
    component: HomeIndex,
    meta: {
      title: 'Image Data Hiding',
      description:
        'Online image Steganography. Securely embed secret messages in images directly from the browser.'
    }
  },
  {
    name: 'about',
    path: '/about',
    component: AboutIndex,
    meta: {
      title: 'About',
      description:
        'Online image Steganography. Notes about the project and how it works.'
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
          description:
            'Online image Steganography. Use text or binary data to protect and hide it in an image.'
        }
      },
      {
        name: 'decode',
        path: 'decode',
        component: DecodeForm,
        meta: {
          title: 'Decode',
          description:
            'Online image Steganography. Use a cover image to read and unprotect its secret message.'
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
