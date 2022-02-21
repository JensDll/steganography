const defaultTheme = require('tailwindcss/defaultTheme')
const Forms = require('@tailwindcss/forms')

const { Icons } = require('./tailwind/plugins/icons')
const { Utils } = require('./tailwind/plugins/utils')

module.exports = {
  content: ['./index.html', './src/**/*.{vue,js,ts,jsx,tsx}'],
  darkMode: 'class',
  theme: {
    extend: {
      fontFamily: {
        sans: ['Montserrat Alternates', ...defaultTheme.fontFamily.sans]
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
