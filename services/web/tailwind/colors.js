const tailwindColors = require('tailwindcss/colors')
const { parseColor } = require('tailwindcss/lib/util/color')

function withOpacityValue(variable) {
  return ({ opacityValue }) => {
    if (opacityValue === undefined) {
      return `rgb(var(${variable}))`
    }
    return `rgb(var(${variable}) / ${opacityValue})`
  }
}

// Delete deprecated colors
delete tailwindColors.lightBlue
delete tailwindColors.warmGray
delete tailwindColors.trueGray
delete tailwindColors.coolGray
delete tailwindColors.blueGray

const colors = {
  ...tailwindColors,
  gray: tailwindColors.slate,
  encode: tailwindColors.emerald,
  decode: tailwindColors.blue,
  'c-border': withOpacityValue('--color-border'),
  'c-bg': withOpacityValue('--color-background'),
  'c-text': withOpacityValue('--color-text'),
  'c-text-error': withOpacityValue('--color-text-error'),
  'c-heading': withOpacityValue('--color-heading'),
  'c-form-bg': withOpacityValue('--color-form-background'),
  'c-form-border': withOpacityValue('--color-form-border'),
  'c-form-focus-border': withOpacityValue('--color-form-focus-border')
}

// Delete aliased colors
delete colors.emerald
delete colors.blue

// Delete unused grays
delete colors.slate
delete colors.zinc
delete colors.neutral
delete colors.stone

const rgb = {}

for (const [colorName, value] of Object.entries(colors)) {
  if (colorName === 'white' || colorName === 'black') {
    const parts = parseColor(value).color.join(' ')
    rgb[colorName] = parts
  } else if (typeof value === 'object') {
    const shades = {}
    rgb[colorName] = shades
    for (const [shade, color] of Object.entries(value)) {
      const parts = parseColor(color).color.join(' ')
      shades[shade] = parts
    }
  }
}

colors.rgb = rgb

module.exports.colors = colors
