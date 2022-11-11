import tailwindColors from 'tailwindcss/colors'
import { parseColor } from 'tailwindcss/lib/util/color'

// Delete deprecated colors
delete (tailwindColors as any).lightBlue
delete (tailwindColors as any).warmGray
delete (tailwindColors as any).trueGray
delete (tailwindColors as any).coolGray
delete (tailwindColors as any).blueGray

export const colors: any = {
  ...tailwindColors,
  gray: tailwindColors.slate,
  encode: tailwindColors.emerald,
  decode: tailwindColors.sky
}

// Delete aliased colors
delete colors.emerald
delete colors.blue

// Delete unused grays
delete colors.slate
delete colors.zinc
delete colors.neutral
delete colors.stone

const rgb: any = {}

for (const [colorName, value] of Object.entries(colors)) {
  if (colorName === 'white' || colorName === 'black') {
    const parts = parseColor(value as string).color.join(' ')
    rgb[colorName] = parts
    continue
  }

  if (value !== null && typeof value === 'object') {
    const shades: Record<string, string> = {}
    rgb[colorName] = shades

    for (const [shade, color] of Object.entries(value)) {
      const parts = parseColor(color).color.join(' ')
      shades[shade] = parts
    }
  }
}

colors.rgb = rgb
