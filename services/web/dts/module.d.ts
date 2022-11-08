declare module 'tailwindcss/lib/util/flattenColorPalette' {
  declare function flattenColorPalette(colors: object): any
  export default flattenColorPalette
}

declare module '@tailwindcss/typography/src/styles' {
  import type { CSSRuleObject } from 'tailwindcss/types/config'
  declare const styles: CSSRuleObject
  export default styles
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
