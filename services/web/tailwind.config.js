const defaultTheme = require('tailwindcss/defaultTheme')
const colors = require('tailwindcss/colors')
const Forms = require('@tailwindcss/forms')

const { Icons } = require('./tailwind/plugins/icons')
const { Utils } = require('./tailwind/plugins/utils')

const { register } = require('esbuild-register/dist/node')
const { unregister } = register()
const { tailwindTheme } = require('./tailwind/theme.ts')
unregister()

/**
 * @type {import('tailwindcss/tailwind-config').TailwindTheme}
 */
module.exports = {
  content: ['./index.html', './src/**/*.{vue,js,ts,jsx,tsx}'],
  darkMode: 'class',
  theme: {
    screen: {
      ...tailwindTheme.screen
    },
    extend: {
      fontFamily: {
        sans: ['Montserrat', ...defaultTheme.fontFamily.sans]
      },
      colors: {
        gray: colors.slate,
        encode: colors.emerald,
        decode: colors.blue
      }
    }
  },
  plugins: [
    Forms,
    Icons({
      'heroicons-outline': ['trash']
    }),
    Utils()
  ]
}
