import { createApp } from 'vue'

import '~/main.css'
import App from '~/App.vue'
import favIconDark from '~/assets/favicon-dark.svg'
import favIcon from '~/assets/favicon.svg'
import { useMediaQuery } from '~/composables'
import { directives } from '~/directives'
import { router } from '~/lib/router'
import { validierung } from '~/lib/validierung'

const app = createApp(App)

app.use(router)
app.use(validierung)
app.use(directives)

app.mount('#app')

const favicon = document.querySelector(
  'head > link[rel="icon"]'
) as HTMLLinkElement

useMediaQuery('(prefers-color-scheme: dark)', matches => {
  if (matches) {
    favicon.setAttribute('href', favIconDark)
  } else {
    favicon.setAttribute('href', favIcon)
  }
})
