import url from 'node:url'

import Vue from '@vitejs/plugin-vue'
import Components from 'unplugin-vue-components/vite'
import { defineConfig } from 'vite'

import { Compression } from './scripts/vite-plugin-compression'

const srcPath = url.fileURLToPath(new url.URL('./src', import.meta.url))

export default defineConfig({
  envDir: 'env',
  resolve: {
    alias: {
      '~': srcPath,
    },
  },
  plugins: [
    Vue(),
    Components({
      dirs: ['src/components', 'src/features/*/components'],
      dts: './env/components.d.ts',
    }),
    Compression(),
  ],
})
