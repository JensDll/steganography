import { watch, reactive, ref, type Ref } from 'vue'

import type { NoInfer } from '~/common/types'

interface Theme {
  text: string
  icon: string
}

interface SystemTheme<TActiveClass extends string> extends Theme {
  get activeClass(): TActiveClass
  get activeIcon(): string
}

export interface CreatedTheme<TThemeName = string> extends Theme {
  name: TThemeName
  isActive: boolean
  activeClass: string
  activeIcon: string
}

export type CreateThemeResult<TThemeName extends string = never> = () => {
  themes: Record<TThemeName, CreatedTheme<TThemeName>>
  activeTheme: Ref<CreatedTheme<TThemeName>>
  changeTheme: (name: TThemeName) => void
}

export function createTheme<TThemeName extends string>(
  themes: Record<TThemeName, Theme>
): CreateThemeResult<TThemeName>

export function createTheme<TThemeName extends string>(
  themes: Record<TThemeName, Theme>,
  systemTheme: SystemTheme<NoInfer<TThemeName>>
): CreateThemeResult<TThemeName | 'system'>

export function createTheme<TThemeName extends string>(
  themes: Record<TThemeName, Theme>,
  systemTheme?: SystemTheme<NoInfer<TThemeName>>
) {
  let createdThemes: Record<string, CreatedTheme> = {}

  for (const [themeName, theme] of Object.entries<Theme>(themes)) {
    createdThemes[themeName] = {
      ...theme,
      name: themeName,
      isActive: localStorage.theme == themeName,
      activeClass: themeName,
      activeIcon: theme.icon
    }
  }

  if (systemTheme) {
    createdThemes.system = {
      ...systemTheme,
      name: 'system',
      isActive: localStorage.theme == 'system'
    }
  }

  const activeTheme = ref<CreatedTheme>(
    createdThemes[localStorage.theme || 'system']
  )

  createdThemes = reactive(createdThemes)

  return () => {
    watch(activeTheme, (theme, prevTheme) => {
      theme.isActive = true
      prevTheme.isActive = false
      localStorage.setItem('theme', theme.name)
      document.documentElement.classList.remove(prevTheme.activeClass)
      document.documentElement.classList.add(theme.activeClass)
      activeTheme.value = theme
    })

    return {
      themes: createdThemes,
      activeTheme
    }
  }
}
