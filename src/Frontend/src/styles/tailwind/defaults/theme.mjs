import tailwindcssTheme from 'tailwindcss/defaultTheme'

import { typography } from './typography'

export const theme = {
  screens: {
    sm: '640px',
    md: '768px',
    lg: '1024px',
    xl: '1280px',
    '2xl': '1536px',
  },
  typography,
  extend: {
    maxWidth: {
      container: 'var(--max-w-container)',
    },
    spacing: {
      container: 'var(--spacing-container)',
    },
    fontFamily: {
      sans: ['Inter var', ...tailwindcssTheme.fontFamily.sans],
    },
  },
}
