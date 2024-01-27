export default defineNuxtConfig({
  devtools: { enabled: false },
  css: ['~/assets/main.css'],
  postcss: {
    plugins: {
      tailwindcss: {},
    },
  },
  routeRules: {
    '/': { redirect: '/encode' },
  },
  nitro: {
    compressPublicAssets: true,
  },
  experimental: {
    inlineSSRStyles: false,
  },
})
