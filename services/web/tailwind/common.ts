import plugin from 'tailwindcss/plugin'

export function Common() {
  return plugin(
    ({ addUtilities, matchUtilities, theme, addBase }) => {
      addBase({
        '.container': {
          width: '100%',
          marginLeft: 'auto',
          marginRight: 'auto',
          maxWidth: theme('maxWidth.container'),
          paddingLeft: theme('padding.container'),
          paddingRight: theme('padding.container')
        }
      })

      addUtilities({
        '.center-children': {
          display: 'grid',
          gridTemplateColumns: '100%',
          gridTemplateRows: '100%',
          placeItems: 'center',
          gridTemplateAreas: "'main'",
          '& > *': {
            gridArea: 'main'
          }
        }
      })

      addUtilities({
        '.firefox-border-animation-fix': {
          border: '0.05px solid rgba(0, 0, 0, 0)',
          backgroundClip: 'padding-box'
        },
        '.safari-overflow-fix': {
          maskImage: 'radial-gradient(white, black)'
        }
      })

      matchUtilities({
        'grid-area': values => {
          return {
            gridArea: values
          }
        }
      })
    },
    {
      corePlugins: {
        container: false
      }
    }
  )
}
