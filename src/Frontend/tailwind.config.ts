import Forms from '@tailwindcss/forms'
import Typography from '@tailwindcss/typography'
import type { Config } from 'tailwindcss'
import { Icons } from 'tailwindcss-plugin-icons'

export default {
  content: {
    relative: true,
    files: ['./index.html', './src/**/*.{vue,js,ts}'],
  },
  darkMode: 'class',
  experimental: {
    optimizeUniversalDefaults: true,
  },
  plugins: [
    Typography(),
    Forms(),
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
  ],
  corePlugins: {
    ringColor: false,
    ringOffsetColor: false,
    ringOffsetWidth: false,
    ringOpacity: false,
    ringWidth: false,
  },
} as Config
