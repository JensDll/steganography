const defaultTheme = require('tailwindcss/defaultTheme')
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
      colors: {
        red: {
          white: 'rgb(255, 250, 250)'
        }
      },
      fontFamily: {
        sans: ['Montserrat Alternates', ...defaultTheme.fontFamily.sans]
      },
      spacing: {
        13: '3.25rem'
      }
    }
  },
  plugins: [
    Forms,
    Icons({
      'heroicons-solid': ['trash'],
      'heroicons-outline': ['trash']
    }),
    Utils()
  ],
  corePlugins: {
    container: false
  }
}
