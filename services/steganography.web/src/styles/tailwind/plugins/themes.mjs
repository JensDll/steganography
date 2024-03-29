import plugin from 'tailwindcss/plugin'

import { colors } from '../defaults/colors.mjs'
import { theme } from '../defaults/theme.mjs'

function withAlphaValue(variable) {
  return `rgb(${asVar(variable)} / <alpha-value>)`
}

function asVar(variable) {
  return `var(${variable})`
}

const base = {
  ...theme.typography.slate.css,
  '--highlight-encode': colors.rgb.encode['500'],
  '--highlight-decode': colors.rgb.decode['500'],
  '--link': colors.rgb.orange['600'],
  '--error': colors.rgb.red['500'],
  '--fill': colors.rgb.gray['50'],
  '--fill-form': colors.rgb.white,
  '--border': colors.rgb.gray['200'],
  '--border-form': colors.rgb.gray['200'],
}

const baseDark = {
  ...theme.typography.slate.css,
  ...theme.typography.invert.css,
  '--highlight-encode': colors.rgb.encode['400'],
  '--highlight-decode': colors.rgb.decode['400'],
  '--link': colors.rgb.orange['500'],
  '--error': colors.rgb.red['600'],
  '--fill': colors.rgb.gray['900'],
  '--fill-form': colors.rgb.gray['800'],
  '--border': colors.rgb.gray['700'],
  '--border-form': colors.rgb.gray['700'],
}

const encode = {
  ...base,
  '--highlight': base['--highlight-encode'],
  '--border-form-highlight': base['--highlight-encode'],
}

const encodeDark = {
  ...baseDark,
  '--highlight': baseDark['--highlight-encode'],
  '--border-form-highlight': colors.rgb.encode['600'],
}

const decode = {
  ...base,
  '--highlight': base['--highlight-decode'],
  '--border-form-highlight': base['--highlight-decode'],
}

const decodeDark = {
  ...baseDark,
  '--highlight': baseDark['--highlight-decode'],
  '--border-form-highlight': colors.rgb.decode['600'],
}

export function Themes() {
  return plugin(
    ({ addComponents }) => {
      addComponents({
        '.theme-encode': encode,
        '.theme-encode-dark': encodeDark,
        '.theme-decode': decode,
        '.theme-decode-dark': decodeDark,
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
        },
        extend: {
          borderColor: {
            DEFAULT: 'rgb(var(--border))',
          },
        },
      },
    },
  )
}
