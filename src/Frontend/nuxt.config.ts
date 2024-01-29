export default defineNuxtConfig({
  devtools: { enabled: false },
  css: ['~/assets/main.css'],
  postcss: {
    plugins: {
      tailwindcss: {},
    },
  },
  imports: {
    dirs: ['api'],
  },
  routeRules: {
    '/': { redirect: '/encode' },
  },
  nitro: {
    compressPublicAssets: true,
  },
  features: {
    inlineStyles: false,
  },
})
