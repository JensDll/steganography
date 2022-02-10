import FormPlugin from '@tailwindcss/forms'
import { type TailwindConfig } from 'tailwindcss/tailwind-config'
import defaultTheme from 'tailwindcss/defaultTheme'

import { GridAreaPlugin } from './plugins/gridArea'
import { AnimationPlugin } from './plugins/animation'

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
  plugins: [FormPlugin, GridAreaPlugin, AnimationPlugin]
}
