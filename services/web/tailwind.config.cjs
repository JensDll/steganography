const Forms = require('@tailwindcss/forms')
const Typography = require('@tailwindcss/typography')
const { register } = require('esbuild-register/dist/node')
const { Icons } = require('tailwindcss-plugin-icons')

const { unregister } = register()
const { AppStyles } = require('./src/styles/tailwind/plugin/appStyles')
const { AppThemes } = require('./src/styles/tailwind/plugin/appThemes')
const { theme } = require('./src/styles/tailwind/theme')
unregister()

/** @type {import('tailwindcss').Config} */
module.exports = {
  content: ['./index.html', './src/**/*.{vue,js,ts,jsx,tsx}'],
  darkMode: 'class',
  theme,
  experimental: {
    optimizeUniversalDefaults: true
  },
  plugins: [
    Typography(),
    Forms(),
    Icons(() => ({
      heroicons: {
        icons: {
          'trash?bg': {}
        },
        includeAll: true,
        scale: iconName => (iconName.endsWith('-20-solid') ? 1.25 : 1.5)
      },
      mdi: {
        includeAll: true,
        scale: 1.5
      },
      custom: {
        includeAll: true,
        scale: 1.5,
        location:
          'https://gist.githubusercontent.com/JensDll/4e59cf6005f585581975941a94bc1d88/raw/0e70bdac81224add27d8f0576ab15406709e5938/icons.json'
      }
    })),
    AppStyles(),
    AppThemes()
  ],
  corePlugins: {
    ringColor: false,
    ringOffsetColor: false,
    ringOffsetWidth: false,
    ringOpacity: false,
    ringWidth: false
  }
}
