import type { Config } from 'tailwindcss'
import colors from 'tailwindcss/colors'
import { Icons } from 'tailwindcss-plugin-icons'

import { Common } from './tailwind.common'
import { Form } from './tailwind.form'

export default {
  content: {
    relative: true,
    files: [
      './components/**/*.{js,vue,ts}',
      './layouts/**/*.vue',
      './pages/**/*.vue',
      './plugins/**/*.{js,ts}',
      './app.vue',
      './error.vue',
    ],
  },
  theme: {
    colors: {
      transparent: 'transparent',
      current: 'currentColor',
      black: colors.black,
      white: colors.white,
      gray: colors.slate,
      red: colors.red,
      indigo: colors.indigo,
      blue: colors.blue,
      green: colors.emerald,
    },
    fontFamily: {
      sans: ['Neon var'],
    },
    extend: {
      maxWidth: {
        container: 'var(--max-w-container)',
      },
      spacing: {
        container: 'var(--spacing-container)',
      },
      ringWidth: {
        3: '3px',
      },
      fontSize: {
        0: '0',
      },
    },
  },
  plugins: [
    Icons(() => ({
      heroicons: {
        includeAll: true,
        scale: iconName => (iconName.endsWith('-20-solid') ? 1.25 : 1.5),
      },
      'svg-spinners': {
        includeAll: true,
        scale: 1.5,
      },
    })),
    Common(null),
    Form(null),
  ],
  corePlugins: {
    container: false,
  },
} as Config
