import { createApp } from 'vue'

import App from './App.vue'
import { router } from './modules/router'
import { validierung } from './modules/validierung'
import { directives } from './modules/directives'
import './main.css'

const app = createApp(App)

app.use(router)
app.use(validierung)
app.use(directives)

app.mount('#app')
