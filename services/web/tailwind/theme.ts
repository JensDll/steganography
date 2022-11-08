import typographyTheme from '@tailwindcss/typography/src/styles'
import tailwindcssTheme from 'tailwindcss/defaultTheme'

removeUnusedTypographyStyles(typographyTheme)

export const theme = {
  screens: {
    sm: '640px',
    md: '768px',
    lg: '1024px',
    xl: '1280px',
    '2xl': '1536px'
  },
  typography: typographyTheme,
  extend: {
    maxWidth: {
      container: 'var(--max-w-container)'
    },
    spacing: {
      container: 'var(--spacing-container)'
    },
    fontFamily: {
      sans: ['Inter var', ...tailwindcssTheme.fontFamily.sans]
    }
  }
} as const

export type TailwindTheme = typeof theme

function removeUnusedTypographyStyles(theme: any) {
  for (const [key, value] of Object.entries(theme)) {
    if (/pre|code/.test(key)) {
      delete theme[key]
      continue
    }

    if (value !== null && (typeof value === 'object' || Array.isArray(value))) {
      removeUnusedTypographyStyles(value)
    }
  }
}
