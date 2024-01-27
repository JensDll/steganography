import defaultTheme from 'tailwindcss/defaultTheme'
import plugin from 'tailwindcss/plugin'
import { iconUrl } from 'tailwindcss-plugin-icons'

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
const checkboxChecked = "[type='checkbox']:checked"
const checkboxIndeterminate = "[type='checkbox']:indeterminate"

const radio = "[type='radio']"
const radioChecked = "[type='radio']:checked"

export const Form = plugin.withOptions<null>(() => ({ addBase, theme }) => {
  addBase({
    [[text, textaera].toString()]: {
      display: 'block',
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
      'background-image': iconUrl({
        width: 16,
        height: 16,
        body: '<circle fill="white" cx="8" cy="8" r="3"/>',
      }),
      '@media (forced-colors: active) ': {
        appearance: 'auto',
      },
    },
    [checkboxChecked]: {
      'background-image': iconUrl({
        width: 16,
        height: 16,
        body: '<path fill="white" d="M12.207 4.793a1 1 0 010 1.414l-5 5a1 1 0 01-1.414 0l-2-2a1 1 0 011.414-1.414L6.5 9.086l4.293-4.293a1 1 0 011.414 0z"/>',
      }),
      '@media (forced-colors: active) ': {
        appearance: 'auto',
      },
    },

    [checkboxIndeterminate]: {
      'background-image': iconUrl({
        width: 16,
        height: 16,
        body: '<path stroke="white" stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 8h8"/>',
      }),
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
