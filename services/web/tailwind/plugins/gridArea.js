import plugin from 'tailwindcss/plugin'

export const GridArea = plugin(({ matchUtilities }) => {
  matchUtilities({
    'grid-area': values => {
      return {
        gridArea: values
      }
    }
  })
})
