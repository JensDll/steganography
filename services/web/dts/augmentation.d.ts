declare module 'tailwindcss/tailwind-config' {
  import { TailwindThemeValue } from 'tailwindcss/tailwind-config'

  interface TailwindTheme {
    readonly gridArea?: TailwindThemeValue
  }
}

export {}
