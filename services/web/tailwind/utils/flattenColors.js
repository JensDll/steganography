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
 * @param {Object} colors - The nested object of colors.
 */
export function flattenColors(colors) {
  const result = {}
  _flattenColors(colors, result)
  return result
}
