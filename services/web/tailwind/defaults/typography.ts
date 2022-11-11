import defaultTypographyTheme from '@tailwindcss/typography/src/styles'

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

removeUnusedTypographyStyles(defaultTypographyTheme)

export const typographyTheme = defaultTypographyTheme as any
