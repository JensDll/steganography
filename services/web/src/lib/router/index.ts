import { type RouteRecordRaw, createRouter, createWebHistory } from 'vue-router'

const routes: RouteRecordRaw[] = [
  {
    name: 'home',
    path: '/',
    component: () => import('~/features/home/routes/HomeView.vue'),
    meta: {
      title: 'Image Data Hiding'
    }
  },
  {
    name: 'about',
    path: '/about',
    component: () => import('~/features/about/routes/AboutView.vue'),
    meta: {
      title: 'About'
    }
  },
  {
    name: 'codec',
    path: '/codec',
    redirect: { name: 'encode' },
    component: () => import('~/features/codec/routes/CodecView.vue'),
    children: [
      {
        name: 'encode',
        path: 'encode',
        alias: '',
        component: () => import('~/features/codec/routes/EncodeView.vue'),
        meta: {
          title: 'Encode'
        }
      },
      {
        name: 'decode',
        path: 'decode',
        component: () => import('~/features/codec/routes/DecodeView.vue'),
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
