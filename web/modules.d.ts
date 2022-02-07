declare module '@tailwindcss/forms' {
  import { TailwindPlugin } from 'tailwindcss/plugin'
  declare const Forms: TailwindPlugin
  export default Forms
}

declare module 'tailwindcss/lib/util/flattenColorPalette' {
  export default (colors: object) => any
}

declare module 'tailwindcss/lib/util/color' {
  type Color = {
    mode: 'rgb' | 'hsl'
    color: [string, string, string]
    alpha?: string
  }

  function parseColor(value: string): Color

  function formatColor(color: Color): string
}
