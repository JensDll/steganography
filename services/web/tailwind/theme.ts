import defaultTheme from 'tailwindcss/defaultTheme'

export const theme = {
  screens: {
    sm: '640px',
    md: '768px',
    lg: '1024px',
    xl: '1280px',
    '2xl': '1536px'
  },
  extend: {
    spacing: {
      container: 'var(--spacing-container)'
    },
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
} as const

export type TailwindTheme = typeof theme
