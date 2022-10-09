const plugin = require('tailwindcss/plugin')

module.exports.Variants = function () {
  return plugin(({ addVariant }) => {
    addVariant(
      'supports-backdrop-blur',
      '@supports (backdrop-filter: blur(4px))'
    )
  })
}
