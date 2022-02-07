import fs from 'node:fs'
import path from 'node:path'

import Forms from '@tailwindcss/forms'
import { optimize } from 'svgo'
import plugin from 'tailwindcss/plugin'
import { TailwindConfig } from 'tailwindcss/tailwind-config'

function readIcons() {
  const icons: Record<string, (color: string) => string> = {}

  for (const file of fs.readdirSync('./scripts/icons')) {
    const result = optimize(fs.readFileSync(`./scripts/icons/${file}`))

    if (result.modernError === undefined) {
      icons[path.parse(file).name] = color => {
        const base64Icon = Buffer.from(
          result.data.replace(/__STROKE__/g, color)
        ).toString('base64')

        return `url('data:image/svg+xml;base64,${base64Icon}')`
      }
    } else {
      console.log(result.modernError)
    }
  }

  return icons
}

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

function flattenColors(
  colors: Record<string, unknown>
): Record<string, string> {
  const result = {}
  _flattenColors(colors, result)
  return result
}

export const config: TailwindConfig = {
  content: ['./index.html', './src/**/*.{vue,js,ts,jsx,tsx}'],
  darkMode: 'media',
  theme: {
    fontFamily: {
      sans: ['Montserrat', 'sans-serif']
    },
    extend: {}
  },
  plugins: [
    Forms,
    // plugin(({ matchUtilities, theme }) => {
    //   const icons = readIcons()

    //   matchUtilities(
    //     {
    //       'bg-lock': (color: string) => {
    //         return {
    //           backgroundImage: icons.lock(color)
    //         }
    //       }
    //     },
    //     {
    //       values: flattenColors(theme('colors')),
    //       type: ['color', 'any']
    //     }
    //   )
    // }),
    plugin(({ matchUtilities }) => {
      matchUtilities({
        'grid-area': (values: string) => {
          return {
            gridArea: values
          }
        }
      })
    })
  ]
}
