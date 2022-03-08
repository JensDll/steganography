import { createApp } from 'vue'
import { gsap } from 'gsap'
import { SlowMo } from 'gsap/EasePack'

import App from './App.vue'
import { pinia } from './modules/pinia'
import { router } from './modules/router'
import { validierung } from './modules/validierung'
import './main.css'

gsap.registerPlugin(SlowMo)

const app = createApp(App)

app.use(pinia)
app.use(router)
app.use(validierung)

app.mount('#app')
