import type { Config } from 'tailwindcss'
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
  experimental: {
    optimizeUniversalDefaults: true,
  },
  theme: {
    fontSize: {
      xs: ['0.75rem', { lineHeight: '1rem' }],
      sm: ['0.875rem', { lineHeight: '1.5rem' }],
      base: ['1rem', { lineHeight: '1.75rem' }],
      lg: ['1.125rem', { lineHeight: '2rem' }],
      xl: ['1.25rem', { lineHeight: '2rem' }],
      '2xl': ['1.5rem', { lineHeight: '2rem' }],
      '3xl': ['2rem', { lineHeight: '2.5rem' }],
      '4xl': ['2.5rem', { lineHeight: '3.5rem' }],
      '5xl': ['3rem', { lineHeight: '3.5rem' }],
      '6xl': ['3.75rem', { lineHeight: '1' }],
      '7xl': ['4.5rem', { lineHeight: '1.1' }],
      '8xl': ['6rem', { lineHeight: '1' }],
      '9xl': ['8rem', { lineHeight: '1' }],
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
        icons: {
          'trash?bg': {},
        },
        includeAll: true,
        scale: iconName => (iconName.endsWith('-20-solid') ? 1.25 : 1.5),
      },
      custom: {
        includeAll: true,
        scale: 1.5,
        location:
          'https://gist.githubusercontent.com/JensDll/4e59cf6005f585581975941a94bc1d88/raw/0e70bdac81224add27d8f0576ab15406709e5938/icons.json',
      },
    })),
    Form(null),
    Common(null),
  ],
  corePlugins: {
    container: false,
  },
} as Config
