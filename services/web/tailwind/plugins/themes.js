const tailwindColors = require('tailwindcss/colors')
const { parseColor } = require('tailwindcss/lib/util/color')
const plugin = require('tailwindcss/plugin')

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
  decode: tailwindColors.blue
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

function withAlphaValue(variable) {
  return `rgb(${asVar(variable)} / <alpha-value>)`
}

function asVar(variable) {
  return `var(${variable})`
}

const themeEncode = {
  '--highlight': rgb.encode['500'],
  '--error': rgb.red['500'],
  '--fill': rgb.gray['50'],
  '--fill-form': rgb.white,
  '--border': rgb.gray['200'],
  '--border-form': rgb.gray['200'],
  '--border-form-highlight': rgb.encode['500']
}

const themeEncodeDark = {
  '--highlight': rgb.encode['500'],
  '--error': rgb.red['400'],
  '--fill': rgb.gray['900'],
  '--fill-form': rgb.gray['800'],
  '--border': rgb.gray['800'],
  '--border-form': rgb.gray['700'],
  '--border-form-highlight': rgb.encode['500']
}

const themeDecode = {
  '--highlight': rgb.decode['500'],
  '--error': rgb.red['500'],
  '--fill': rgb.gray['50'],
  '--fill-form': rgb.white,
  '--border': rgb.gray['200'],
  '--border-form': rgb.gray['200'],
  '--border-form-highlight': rgb.decode['500']
}

const themeDecodeDark = {
  '--highlight': rgb.decode['500'],
  '--error': rgb.red['400'],
  '--fill': rgb.gray['900'],
  '--fill-form': rgb.gray['800'],
  '--border': rgb.gray['800'],
  '--border-form': rgb.gray['700'],
  '--border-form-highlight': rgb.decode['500']
}

module.exports.Themes = function () {
  return plugin(
    ({ addComponents }) => {
      addComponents({
        '.theme-encode': themeEncode,
        '.theme-encode-dark': themeEncodeDark,
        '.theme-decode': themeDecode,
        '.theme-decode-dark': themeDecodeDark
      })
    },
    {
      theme: {
        colors: {
          ...colors,
          ...Object.entries(themeEncode).reduce((result, [key]) => {
            result[key.replace('--', '')] = withAlphaValue(key)
            return result
          }, {}),
          rgb
        },
        extend: {
          borderColor: {
            DEFAULT: 'rgb(var(--border))'
          }
        }
      }
    }
  )
}
