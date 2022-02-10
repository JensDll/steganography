import plugin from 'tailwindcss/plugin'

export const GridAreaPlugin = plugin(({ matchUtilities }) => {
  matchUtilities({
    'grid-area': values => {
      return {
        gridArea: values
      }
    }
  })
})
