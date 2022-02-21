const plugin = require('tailwindcss/plugin')

module.exports.Utils = function () {
  return plugin(({ addUtilities, matchUtilities }) => {
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

    matchUtilities({
      'grid-area': values => {
        return {
          gridArea: values
        }
      }
    })
  })
}
