function _flattenColors(
  colors: object,
  result: Record<string, unknown>,
  path: string[] = []
) {
  for (const [key, value] of Object.entries(colors)) {
    const pathWithCurrentKey = [...path, key]

    if (typeof value === 'object') {
      _flattenColors(value, result, pathWithCurrentKey)
    } else {
      result[pathWithCurrentKey.join('-')] = value
    }
  }
}

export function flattenColors(
  colors: Record<string, unknown>
): Record<string, string> {
  const result = {}
  _flattenColors(colors, result)
  return result
}
