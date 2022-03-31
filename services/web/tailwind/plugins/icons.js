const plugin = require('tailwindcss/plugin')

const customIcons = require('./icons.json')
const { flattenColors } = require('../utils/flattenColors')

function toBase64Url(body, dimension, color) {
  const colorReplacedBody = body.replace(/currentColor/g, color)
  const icon = `<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 ${dimension} ${dimension}">${colorReplacedBody}</svg>`
  const base64Icon = Buffer.from(icon).toString('base64')

  return `url('data:image/svg+xml;base64,${base64Icon}')`
}

module.exports.Icons = function (options) {
  const iconUtilities = {}

  for (const [iconSetName, iconNames] of Object.entries(options)) {
    const iconSet = require(`@iconify-json/${iconSetName}/icons.json`)

    for (const iconName of iconNames) {
      iconUtilities[`bg-${iconSetName}-${iconName}`] = color => {
        return {
          backgroundImage: toBase64Url(
            iconSet.icons[iconName].body,
            iconSet.width,
            color
          )
        }
      }
    }
  }

  for (const [iconName, { body, dimension }] of Object.entries(customIcons)) {
    iconUtilities[`bg-custom-${iconName}`] = color => {
      return {
        backgroundImage: toBase64Url(body, dimension, color)
      }
    }
  }

  return plugin(({ matchUtilities, theme }) => {
    matchUtilities(iconUtilities, {
      values: flattenColors(theme('colors')),
      type: ['color', 'any']
    })
  })
}
