declare module 'tailwindcss/tailwind-config' {
  import { TailwindThemeValue } from 'tailwindcss/tailwind-config'

  interface TailwindTheme {
    readonly gridArea?: TailwindThemeValue
  }
}

declare module '@vue/runtime-core' {
  interface ComponentCustomProperties {
    APP_CONFIG: typeof APP_CONFIG
  }
}

export {}
