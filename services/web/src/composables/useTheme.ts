import { computed, ref, watch } from 'vue'

export type ThemeName = 'light' | 'dark' | 'system'

export type Theme = {
  theme: ThemeName
  text: string
  icon: string
}

const themes: Theme[] = [
  {
    theme: 'light',
    text: 'Light',
    icon: 'i-ic-twotone-wb-sunny'
  },
  {
    theme: 'dark',
    text: 'Dark',
    icon: 'i-ic-twotone-dark-mode'
  },
  {
    theme: 'system',
    text: 'System',
    icon: 'i-ic-twotone-computer'
  }
]

export function useTheme() {
  const activeTheme = ref<ThemeName>(localStorage.theme)
  const isDark = ref(document.documentElement.classList.contains('dark'))
  const isLight = computed(() => !isDark.value)

  const changeTheme = (theme: ThemeName) => {
    $changeTheme(theme)
    activeTheme.value = theme
    isDark.value = document.documentElement.classList.contains('dark')
  }

  watch(activeTheme, changeTheme)

  return {
    activeTheme,
    themes,
    isDark,
    isLight,
    changeTheme
  }
}
