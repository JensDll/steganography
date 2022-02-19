import { type TailwindConfig } from 'tailwindcss/tailwind-config'
import defaultTheme from 'tailwindcss/defaultTheme'
import plugin from 'tailwindcss/plugin'
import Form from '@tailwindcss/forms'

export const config: TailwindConfig = {
  content: ['./index.html', './src/**/*.{vue,js,ts,jsx,tsx}'],
  darkMode: 'media',
  theme: {
    keyframes: {
      bounce: {
        '0%': {
          animationTimingFunction: 'cubic-bezier(0, 0, 0.2, 1)'
        },
        '50%': {
          transform: 'translateY(-25%)',
          animationTimingFunction: 'cubic-bezier(0.8, 0, 1, 1)'
        },
        '100%': {
          animationTimingFunction: 'cubic-bezier(0, 0, 0.2, 1)'
        }
      }
    },
    animation: {
      bounce: '1s bounce var(--tw-iteration-count, infinite)'
    },
    extend: {
      fontFamily: {
        sans: ['Montserrat Alternates', ...defaultTheme.fontFamily.sans]
      }
    }
  },
  plugins: [
    Form,
    plugin(({ matchUtilities }) => {
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
    }),
    plugin(({ matchUtilities }) => {
      matchUtilities({
        'grid-area': values => {
          return {
            gridArea: values
          }
        }
      })
    })
  ],
  corePlugins: {
    container: false
  } as never
}
