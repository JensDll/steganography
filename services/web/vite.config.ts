import url from 'url'

import Vue from '@vitejs/plugin-vue'
import Components from 'unplugin-vue-components/vite'
import { defineConfig } from 'vite'

const srcPath = url.fileURLToPath(new url.URL('./src', import.meta.url))
const tailwindThemePath = url.fileURLToPath(
  new url.URL('./tailwind/theme', import.meta.url)
)

export default defineConfig({
  resolve: {
    alias: {
      '~': srcPath,
      'tailwind-theme': tailwindThemePath
    }
  },
  plugins: [
    Vue(),
    Components({
      dirs: ['src/components', 'src/features/*/components'],
      dts: './dts/components.d.ts'
    })
  ]
})
