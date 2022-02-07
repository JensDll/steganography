import url from 'url'

import { defineConfig } from 'vite'
import Vue from '@vitejs/plugin-vue'
import Components from 'unplugin-vue-components/vite'
import Icons from 'unplugin-icons/vite'
import IconsResolver from 'unplugin-icons/resolver'

export default defineConfig({
  resolve: {
    alias: {
      '~': url.fileURLToPath(new url.URL('./src', import.meta.url))
    }
  },
  plugins: [
    Vue(),
    Components({
      resolvers: [
        IconsResolver({
          componentPrefix: ''
        })
      ]
    }),
    Icons({
      autoInstall: true
    })
  ]
})
