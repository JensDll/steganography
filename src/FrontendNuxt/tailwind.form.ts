import defaultTheme from 'tailwindcss/defaultTheme'
import plugin from 'tailwindcss/plugin'

const {
  spacing,
  borderWidth,
  borderRadius,
  fontSize: {
    base: [baseFontSize, { lineHeight: baseLineHeight }],
  },
} = defaultTheme

export const Form = plugin.withOptions<null>(() => ({ addBase }) => {
  addBase({
    [["[type='text']", 'textarea'].toString()]: {
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
  })
})
