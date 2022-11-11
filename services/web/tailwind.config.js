const Forms = require('@tailwindcss/forms')
const Typography = require('@tailwindcss/typography')
const { register } = require('esbuild-register/dist/node')
const { Icons, SCALE } = require('tailwindcss-plugin-icons')

const { unregister } = register()
const { Common } = require('./tailwind/common')
const { theme } = require('./tailwind/theme')
const { Themes } = require('./tailwind/themes')
const { Variants } = require('./tailwind/variants')
unregister()

/** @type {import('tailwindcss').Config} */
module.exports = {
  content: ['./index.html', './src/**/*.{vue,js,ts,jsx,tsx}'],
  darkMode: 'class',
  theme,
  plugins: [
    Typography(),
    Forms(),
    Icons(() => ({
      heroicons: {
        icons: {
          trash: {},
          'trash?bg': {},
          'ellipsis-vertical': {},
          'x-mark': {},
          moon: {},
          sun: {},
          'x-circle': {},
          'computer-desktop': {},
          'paper-clip-20-solid': {
            [SCALE]: 1.25
          },
          'lock-open-20-solid': {
            [SCALE]: 1.25
          },
          'lock-closed-20-solid': {
            [SCALE]: 1.25
          },
          'x-mark-20-solid': {
            [SCALE]: 1.25
          },
          'x-circle-20-solid': {
            [SCALE]: 1.25
          },
          'arrow-left-20-solid': {
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
    Common(),
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
