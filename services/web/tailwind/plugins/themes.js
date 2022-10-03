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

const base = {
  '--highlight-encode': rgb.encode['500'],
  '--highlight-decode': rgb.decode['500'],
  '--link': rgb.orange['500'],
  '--error': rgb.red['500'],
  '--fill': rgb.gray['50'],
  '--fill-form': rgb.white,
  '--border': rgb.gray['200'],
  '--border-form': rgb.gray['200']
}

const baseDark = {
  '--highlight-encode': rgb.encode['400'],
  '--highlight-decode': rgb.decode['400'],
  '--link': rgb.orange['400'],
  '--error': rgb.red['500'],
  '--fill': rgb.gray['900'],
  '--fill-form': rgb.gray['800'],
  '--border': rgb.gray['800'],
  '--border-form': rgb.gray['700']
}

const encode = {
  ...base,
  '--highlight': base['--highlight-encode'],
  '--border-form-highlight': base['--highlight-encode']
}

const encodeDark = {
  ...baseDark,
  '--highlight': baseDark['--highlight-encode'],
  '--border-form-highlight': rgb.encode['600']
}

const decode = {
  ...base,
  '--highlight': base['--highlight-decode'],
  '--border-form-highlight': base['--highlight-decode']
}

const decodeDark = {
  ...baseDark,
  '--highlight': baseDark['--highlight-decode'],
  '--border-form-highlight': rgb.decode['600']
}

module.exports.Themes = function () {
  return plugin(
    ({ addComponents }) => {
      addComponents({
        '.theme-encode': encode,
        '.theme-encode-dark': encodeDark,
        '.theme-decode': decode,
        '.theme-decode-dark': decodeDark
      })
    },
    {
      theme: {
        colors: {
          ...colors,
          ...Object.entries(encode).reduce((result, [key]) => {
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
