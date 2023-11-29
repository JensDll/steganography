import { createApp } from 'vue'

import './main.css'
import App from '~/App.vue'
import { router } from '~/deps/router'
import { validierung } from '~/deps/validierung'

const app = createApp(App)

app.use(router)
app.use(validierung)

app.mount('#app')
