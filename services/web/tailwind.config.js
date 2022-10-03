const Forms = require('@tailwindcss/forms')
const Typography = require('@tailwindcss/typography')
const { register } = require('esbuild-register/dist/node')
const { Icons, SCALE } = require('tailwindcss-plugin-icons')
const defaultTheme = require('tailwindcss/defaultTheme')

const { Themes } = require('./tailwind/plugins/themes')
const { Utilities } = require('./tailwind/plugins/utilities')
const { Variants } = require('./tailwind/plugins/variants')
const { unregister } = register()
const { tailwindTheme } = require('./tailwind/theme.ts')
unregister()

/** @type {import('tailwindcss').Config} */
module.exports = {
  content: ['./index.html', './src/**/*.{vue,js,ts,jsx,tsx}'],
  darkMode: 'class',
  theme: {
    screens: tailwindTheme.screens,
    extend: {
      fontFamily: {
        sans: ['Inter', ...defaultTheme.fontFamily.sans]
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
    Forms(),
    Typography(),
    Icons(() => ({
      heroicons: {
        icons: {
          trash: {},
          'trash?bg': {},
          'ellipsis-vertical': {},
          'x-mark': {},
          moon: {},
          sun: {},
          'computer-desktop': {},
          'paper-clip-20-solid': {
            [SCALE]: 1.25
          },
          'lock-open-20-solid': {
            [SCALE]: 1.25
          },
          'lock-closed-20-solid': {
            [SCALE]: 1.25
          }
        },
        scale: 1.5
      },
      mdi: {
        icons: {
          github: {}
        },
        scale: 1.5
      },
      custom: {
        icons: {
          loading: {}
        },
        scale: 1.5,
        location:
          'https://gist.githubusercontent.com/JensDll/4e59cf6005f585581975941a94bc1d88/raw/0e70bdac81224add27d8f0576ab15406709e5938/icons.json'
      }
    })),
    Utilities(),
    Variants(),
    Themes()
  ],
  corePlugins: {
    ringColor: false,
    ringOffsetColor: false,
    ringOffsetWidth: false,
    ringOpacity: false,
    ringWidth: false
  }
}
