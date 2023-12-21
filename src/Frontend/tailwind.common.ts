import plugin from 'tailwindcss/plugin'

export const Common = plugin.withOptions(
  () =>
    ({ addUtilities, matchUtilities, theme }) => {
      addUtilities({
        '.container': {
          width: '100%',
          marginLeft: 'auto',
          marginRight: 'auto',
          maxWidth: theme('maxWidth.container'),
          paddingLeft: theme('padding.container'),
          paddingRight: theme('padding.container'),
        },
      })

      addUtilities({
        '.center-children': {
          display: 'grid',
          gridTemplateColumns: '100%',
          gridTemplateRows: '100%',
          placeItems: 'center',
          gridTemplateAreas: "'main'",
          '& > *': {
            gridArea: 'main',
          },
        },
      })

      matchUtilities({
        'grid-area': values => {
          return {
            gridArea: values,
          }
        },
      })
    },
)
