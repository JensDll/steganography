import { createApp } from 'vue'

import App from './App.vue'
import { pinia } from './modules/pinia'
import { router } from './modules/router'

const app = createApp(App)

app.use(pinia)
app.use(router)

app.mount('#app')
