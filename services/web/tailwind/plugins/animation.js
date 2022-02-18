import plugin from 'tailwindcss/plugin'

export const Animation = plugin(({ matchUtilities }) => {
  matchUtilities(
    {
      'animate-iteration': timing => {
        return {
          '--tw-iteration-count': timing
        }
      }
    },
    {
      values: {
        infinite: 'infinite'
      }
    }
  )
})
