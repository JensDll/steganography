import { createApp } from 'vue'

import App from './App.vue'
import { router } from './modules/router'
import { validierung } from './modules/validierung'
import { directives } from './modules/directives'
import { useMediaQuery } from './domain'
import './main.css'

console.log('FOO')

const app = createApp(App)

app.use(router)
app.use(validierung)
app.use(directives)

app.mount('#app')

const favicon = document.querySelector(
  'head > link[rel="icon"]'
) as HTMLLinkElement
const faviconAlternate = document.querySelector(
  'head > link[rel="alternate icon"]'
) as HTMLLinkElement

useMediaQuery('(prefers-color-scheme: dark)', matches => {
  if (matches) {
    favicon.setAttribute('href', '/logo.svg')
    faviconAlternate.setAttribute('href', '/logo-dark.ico')
  } else {
    favicon.setAttribute('href', '/logo.svg')
    faviconAlternate.setAttribute('href', '/logo.ico')
  }
})
