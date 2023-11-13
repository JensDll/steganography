declare module 'tailwindcss/tailwind-config' {
  import { TailwindThemeValue } from 'tailwindcss/tailwind-config'

  interface TailwindTheme {
    readonly gridArea?: TailwindThemeValue
  }
}

declare module '@vue/runtime-core' {
  interface ComponentCustomProperties {
    $config: typeof $config
  }
}

export {}
