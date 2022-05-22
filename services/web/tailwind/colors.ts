import tailwindColors from "tailwindcss/colors"
import { parseColor } from 'tailwindcss/lib/util/color'
import type { TailwindColorGroup } from 'tailwindcss/tailwind-config'

type DeprecatedTailwindColor =
  | 'lightBlue'
  | 'warmGray'
  | 'trueGray'
  | 'coolGray'
  | 'blueGray'

type TailwindColorsNotDeprecated = Omit<
  typeof tailwindColors,
  DeprecatedTailwindColor
>

// Delete deprecated colors
// @ts-expect-error Ignore
delete tailwindColors.lightBlue
// @ts-expect-error Ignore
delete tailwindColors.warmGray
// @ts-expect-error Ignore
delete tailwindColors.trueGray
// @ts-expect-error Ignore
delete tailwindColors.coolGray
// @ts-expect-error Ignore
delete tailwindColors.blueGray

function withOpacityValue(variable: string) {
  return ({ opacityValue }: any) => {
    if (opacityValue === undefined) {
      return `rgb(var(${variable}))`
    }
    return `rgb(var(${variable}) / ${opacityValue})`
  }
}

function variable(variable: string) {
  return `var(${variable})`
}

function generateColors(baseColors: TailwindColorsNotDeprecated): any {
  const colors: Record<string, unknown> = {
    ...baseColors,
    gray: baseColors.slate,
    encode: baseColors.emerald,
    decode: baseColors.blue
  }

  // Delete aliased colors
  delete colors.emerald
  delete colors.blue

  // Delete unused grays
  delete colors.slate
  delete colors.zinc
  delete colors.neutral
  delete colors.stone

  const rgb: Record<string, string | TailwindColorGroup> = {}

  for (const [colorName, value] of Object.entries<any>(colors)) {
    if (colorName === 'white' || colorName === 'black') {
      const parts = parseColor(value as string).color.join(' ')
      rgb[colorName] = parts
    } else if (typeof value === 'object') {
      const shades: Record<string, string> = {}
      rgb[colorName] = shades
      for (const [shade, color] of Object.entries<any>(value)) {
        const parts = parseColor(color).color.join(' ')
        shades[shade] = parts
      }
    }
  }

  colors.rgb = rgb

  colors.text = {
    base: variable('--tw-prose-body'),
    error: withOpacityValue('--color-text-error'),
    heading: variable('--tw-prose-headings')
  }
  colors.bg = {
    base: withOpacityValue('--color-fill'),
    form: {
      base: withOpacityValue('--color-form-fill')
    }
  }
  colors.border = {
    base: withOpacityValue('--color-border-base'),
    form: {
      base: withOpacityValue('--color-border-form-base'),
      highlight: withOpacityValue('--color-border-form-highlight')
    }
  }

  return colors
}

export const colors = generateColors(tailwindColors)
