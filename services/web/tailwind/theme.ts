export const tailwindTheme = {
  screen: {
    sm: '640px',
    md: '768px',
    lg: '1024px',
    xl: '1280px',
    '2xl': '1536px'
  }
} as const

export type TailwindTheme = typeof tailwindTheme
