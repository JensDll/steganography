import plugin from 'tailwindcss/plugin'

export function Variants() {
  return plugin(({ addVariant }) => {
    addVariant(
      'supports-backdrop-blur',
      '@supports (backdrop-filter: blur(4px))'
    )
  })
}
