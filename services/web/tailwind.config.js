const defaultTheme = require('tailwindcss/defaultTheme')
const Forms = require('@tailwindcss/forms')
const Typography = require('@tailwindcss/typography')
const { Icons } = require('tailwindcss-plugin-icons')
const { register } = require('esbuild-register/dist/node')

const { Utilities } = require('./tailwind/plugins/utilities')
const { Variants } = require('./tailwind/plugins/variants')
const { colors } = require('./tailwind/colors')
const { unregister } = register()
const { tailwindTheme } = require('./tailwind/theme.ts')
unregister()

/** @type {import('tailwindcss').Config} */
module.exports = {
  content: ['./index.html', './src/**/*.{vue,js,ts,jsx,tsx}'],
  darkMode: 'class',
  theme: {
    colors,
    screens: tailwindTheme.screens,
    extend: {
      borderColor: {
        DEFAULT: 'rgb(var(--color-border-base))'
      },
      fontFamily: {
        sans: ['Montserrat', ...defaultTheme.fontFamily.sans]
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
    Icons(() => ({
      heroiconsOutline: {
        icons: {
          trash: {},
          'trash?bg': {},
          dotsVertical: {},
          x: {},
          moon: {},
          sun: {},
          desktopComputer: {}
        },
        scale: 1.5
      },
      heroiconsSolid: {
        icons: {
          paperClip: {},
          lockOpen: {},
          lockClosed: {}
        },
        scale: 1.25
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
