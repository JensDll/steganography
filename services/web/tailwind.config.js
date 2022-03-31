const defaultTheme = require('tailwindcss/defaultTheme')
const Forms = require('@tailwindcss/forms')
const Typography = require('@tailwindcss/typography')

const { Icons } = require('./tailwind/plugins/icons')
const { Utils } = require('./tailwind/plugins/utils')
const { Variants } = require('./tailwind/plugins/variants')

const { register } = require('esbuild-register/dist/node')
const { unregister } = register()
const { colors } = require('./tailwind/colors.ts')
const { tailwindTheme } = require('./tailwind/theme.ts')
unregister()

/**
 * @type {import('tailwindcss/tailwind-config').TailwindTheme}
 */
module.exports = {
  content: ['./index.html', './src/**/*.{vue,js,ts,jsx,tsx}'],
  darkMode: 'class',
  theme: {
    colors,
    screens: tailwindTheme.screens,
    extend: {
      fontFamily: {
        sans: ['Montserrat', ...defaultTheme.fontFamily.sans]
      },
      boxShadow: {
        none: 'initial'
      },
      typography: {
        DEFAULT: {
          css: {
            'code::before': {
              content: '""'
            },
            'code::after': {
              content: '""'
            }
          }
        }
      }
    }
  },
  plugins: [
    Forms,
    Typography,
    Icons({
      'heroicons-outline': ['trash']
    }),
    Utils(),
    Variants()
  ],
  corePlugins: {
    ringColor: false,
    ringOffsetColor: false,
    ringOffsetWidth: false,
    ringOpacity: false,
    ringWidth: false
  }
}
