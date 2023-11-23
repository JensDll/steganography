import defaultTypographyTheme from '@tailwindcss/typography/src/styles'

// Remove default variables from typography theme
defaultTypographyTheme.DEFAULT.css.splice(1, 1)

defaultTypographyTheme.gray = defaultTypographyTheme.slate

export const typography = defaultTypographyTheme
