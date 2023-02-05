import { createTheme } from './createTheme'

export const useTheme = createTheme(
  {
    light: {
      icon: 'i-heroicons-sun',
      text: 'Light'
    },
    dark: {
      icon: 'i-heroicons-moon',
      text: 'Dark'
    }
  },
  {
    text: 'System',
    icon: 'i-heroicons-computer-desktop',
    get activeClass() {
      if (window.matchMedia('(prefers-color-scheme: dark)').matches) {
        return 'dark'
      }

      return 'light'
    },
    get activeIcon() {
      if (document.documentElement.classList.contains('dark')) {
        return 'i-heroicons-moon'
      }

      return 'i-heroicons-sun'
    }
  }
)
