import { createApp } from 'vue'

import App from './App.vue'
import { pinia } from './modules/pinia'
import { router } from './modules/router'
import { validierung } from './modules/validierung'
import './css/main.css'

const app = createApp(App)

app.use(pinia)
app.use(router)
app.use(validierung)

app.mount('#app')
