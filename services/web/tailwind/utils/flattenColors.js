/**
 * Flatten colors implementation.
 * @param {object} colors
 * @param {Record<string, any>} result
 * @param {string[]} path
 */
function _flattenColors(colors, result, path = []) {
  for (const [key, value] of Object.entries(colors)) {
    const pathWithCurrentKey = [...path, key]

    if (typeof value === 'object') {
      _flattenColors(value, result, pathWithCurrentKey)
    } else {
      result[pathWithCurrentKey.join('-')] = value
    }
  }
}

/**
 * Flattens a nested object of colors into a single object.
 * @param {object} colors - The nested object of colors.
 */
module.exports.flattenColors = function (colors) {
  const result = {}
  _flattenColors(colors, result)
  return result
}
