const plugin = require('tailwindcss/plugin')

const { flattenColors } = require('../flattenColors')

function encodeSvg(svg) {
  return svg
    .replace(
      '<svg',
      ~svg.indexOf('xmlns') ? '<svg' : '<svg xmlns="http://www.w3.org/2000/svg"'
    )
    .replace(/"/g, "'")
    .replace(/%/g, '%25')
    .replace(/#/g, '%23')
    .replace(/{/g, '%7B')
    .replace(/}/g, '%7D')
    .replace(/</g, '%3C')
    .replace(/>/g, '%3E')
}

function getIconUtility(svgBody, width, height) {
  const svg = `<svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 ${width} ${height}">${svgBody}</svg>`
  const mode = svg.includes('currentColor') ? 'mask' : 'background'
  const uri = `url("data:image/svg+xml;utf8,${encodeSvg(svg)}")`

  if (mode === 'mask') {
    return {
      mask: `${uri} no-repeat`,
      maskSize: '100% 100%',
      backgroundColor: 'currentColor'
    }
  } else {
    return {
      background: `${uri} no-repeat`,
      backgroundSize: '100% 100%',
      backgroundColor: 'transparent'
    }
  }
}

const getBackgroundImageIconUtility = (svgBody, width, height) => color => {
  const colorReplacedBody = svgBody.replace(/currentColor/g, color)
  const svg = `<svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 ${width} ${height}">${colorReplacedBody}</svg>`
  const uri = `url("data:image/svg+xml,${encodeSvg(svg)}")`

  return {
    background: `${uri} no-repeat`,
    backgroundSize: '100% 100%'
  }
}

module.exports.Icons = function (iconSets, backgroundImageIconSets) {
  const iconUtilities = {}
  const backgroundImageIconUtilities = {}

  for (const [iconSetName, iconNames] of Object.entries(iconSets)) {
    const iconSet = require(iconSetName === 'custom'
      ? './icons.json'
      : `@iconify-json/${iconSetName}/icons.json`)

    for (const iconName of iconNames) {
      iconUtilities[`.i-${iconSetName}-${iconName}`] = getIconUtility(
        iconSet.icons[iconName].body,
        iconSet.width,
        iconSet.height
      )
    }
  }

  for (const [iconSetName, iconNames] of Object.entries(
    backgroundImageIconSets
  )) {
    const iconSet = require(iconSetName === 'custom'
      ? './icons.json'
      : `@iconify-json/${iconSetName}/icons.json`)

    for (const iconName of iconNames) {
      backgroundImageIconUtilities[`bg-${iconSetName}-${iconName}`] =
        getBackgroundImageIconUtility(
          iconSet.icons[iconName].body,
          iconSet.width,
          iconSet.height
        )
    }
  }

  return plugin(({ addUtilities, matchUtilities, theme }) => {
    addUtilities(iconUtilities)

    matchUtilities(backgroundImageIconUtilities, {
      values: flattenColors(theme('colors')),
      type: ['color', 'any']
    })
  })
}
