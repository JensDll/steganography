import defaultTheme from 'tailwindcss/defaultTheme'
import plugin from 'tailwindcss/plugin'

function encodeSvg(svg: string) {
  if (!svg.includes(' xmlns:xlink=') && svg.includes(' xlink:')) {
    svg = svg.replace('<svg', '<svg xmlns:xlink="http://www.w3.org/1999/xlink"')
  }
  if (!svg.includes(' xmlns=')) {
    svg = svg.replace('<svg', '<svg xmlns="http://www.w3.org/2000/svg"')
  }
  return svg
    .replace(/"/g, "'")
    .replace(/%/g, '%25')
    .replace(/#/g, '%23')
    .replace(/</g, '%3C')
    .replace(/>/g, '%3E')
    .replace(/\s+/g, ' ')
}

function iconUrl(svg: string) {
  return `url("data:image/svg+xml,${encodeSvg(svg)}")`
}

const {
  spacing,
  borderWidth,
  borderRadius,
  fontSize: {
    base: [baseFontSize, { lineHeight: baseLineHeight }],
  },
} = defaultTheme

const text = "[type='text']"

const textaera = 'textarea'

const checkbox = "[type='checkbox']"
const checkboxFocus = "[type='checkbox']:focus"
const checkboxChecked = "[type='checkbox']:checked"
const checkboxCheckedHover = "[type='checkbox']:checked:hover"
const checkboxCheckedFocus = "[type='checkbox']:checked:focus"
const checkboxIndeterminate = "[type='checkbox']:indeterminate"
const checkboxIndeterminateFocus = "[type='checkbox']:indeterminate:focus"
const checkboxIndeterminateHover = "[type='checkbox']:indeterminate:hover"

const radio = "[type='radio']"
const radioFocus = "[type='radio']:focus"
const radioChecked = "[type='radio']:checked"
const radioCheckedHover = "[type='radio']:checked:hover"
const radioCheckedFocus = "[type='radio']:checked:focus"

export const Form = plugin.withOptions<null>(() => ({ addBase, theme }) => {
  addBase({
    [[text, textaera].toString()]: {
      appearance: 'none',
      borderWidth: borderWidth.DEFAULT,
      borderRadius: borderRadius.none,
      paddingTop: spacing[2],
      paddingBottom: spacing[2],
      paddingLeft: spacing[3],
      paddingRight: spacing[3],
      fontSize: baseFontSize,
      lineHeight: baseLineHeight,
    },

    [[radio, checkbox].toString()]: {
      appearance: 'none',
      padding: '0',
      'print-color-adjust': 'exact',
      display: 'inline-block',
      'vertical-align': 'middle',
      'background-origin': 'border-box',
      'user-select': 'none',
      'flex-shrink': '0',
      height: spacing[4],
      width: spacing[4],
      'background-color': '#fff',
      'border-width': borderWidth['DEFAULT'],
    },
    [radio]: {
      'border-radius': '100%',
    },

    [[radioChecked, checkboxChecked].toString()]: {
      'border-color': `currentColor`,
      'background-color': `currentColor`,
      'background-size': `100% 100%`,
      'background-position': `center`,
      'background-repeat': `no-repeat`,
    },
    [radioChecked]: {
      'background-image': iconUrl(
        '<svg viewBox="0 0 16 16" fill="white" xmlns="http://www.w3.org/2000/svg"><circle cx="8" cy="8" r="3"/></svg>',
      ),
      '@media (forced-colors: active) ': {
        appearance: 'auto',
      },
    },
    [checkboxChecked]: {
      'background-image': iconUrl(
        '<svg viewBox="0 0 16 16" fill="white" xmlns="http://www.w3.org/2000/svg"><path d="M12.207 4.793a1 1 0 010 1.414l-5 5a1 1 0 01-1.414 0l-2-2a1 1 0 011.414-1.414L6.5 9.086l4.293-4.293a1 1 0 011.414 0z"/></svg>',
      ),
      '@media (forced-colors: active) ': {
        appearance: 'auto',
      },
    },

    [checkboxIndeterminate]: {
      'background-image': iconUrl(
        '<svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 16 16"><path stroke="white" stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 8h8"/></svg>',
      ),
      'border-color': `transparent`,
      'background-color': `currentColor`,
      'background-size': `100% 100%`,
      'background-position': `center`,
      'background-repeat': `no-repeat`,
      '@media (forced-colors: active) ': {
        appearance: 'auto',
      },
    },
  })
})
