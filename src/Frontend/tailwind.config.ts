import Typography from '@tailwindcss/typography'
import type { Config } from 'tailwindcss'
import { Icons } from 'tailwindcss-plugin-icons'

import { Common } from './tailwind.common'
import { Form } from './tailwind.form'

export default {
  content: {
    relative: true,
    files: ['./index.html', './src/**/*.{vue,js,ts}'],
  },
  experimental: {
    optimizeUniversalDefaults: true,
  },
  theme: {
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
    Typography(),
    Icons(() => ({
      heroicons: {
        icons: {
          'trash?bg': {},
        },
        includeAll: true,
        scale: iconName => (iconName.endsWith('-20-solid') ? 1.25 : 1.5),
      },
      mdi: {
        includeAll: true,
        scale: 1.5,
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
