import tailwindColors from 'tailwindcss/colors'
import { parseColor } from 'tailwindcss/lib/util/color'

const deprecatedColors = [
  'lightBlue',
  'warmGray',
  'trueGray',
  'coolGray',
  'blueGray',
]
deprecatedColors.forEach(color => delete tailwindColors[color])

export const colors = {
  ...tailwindColors,
  gray: tailwindColors.slate,
  encode: tailwindColors.emerald,
  decode: tailwindColors.sky,
}

const unusedGrays = ['slate', 'zinc', 'neutral', 'stone']
unusedGrays.forEach(color => delete colors[color])

const rgb = {}

for (const [colorName, value] of Object.entries(colors)) {
  if (colorName === 'white' || colorName === 'black') {
    const parts = parseColor(value).color.join(' ')
    rgb[colorName] = parts
    continue
  }

  if (value !== null && typeof value === 'object') {
    const shades = {}
    rgb[colorName] = shades

    for (const [shade, color] of Object.entries(value)) {
      const parts = parseColor(color).color.join(' ')
      shades[shade] = parts
    }
  }
}

colors.rgb = rgb
