import { onMounted, onUnmounted, ref } from 'vue'
import type { TailwindTheme } from 'tailwind-theme'

type Query =
  | '(prefers-reduced-motion: reduce)'
  | `(max-width: ${TailwindTheme['screen'][keyof TailwindTheme['screen']]})`

export function useMediaQuery(
  query: Query,
  onUpdate?: (matches: boolean) => void
) {
  const mediaQuery = window.matchMedia(query)
  const matches = ref(false)

  const update = () => {
    matches.value = mediaQuery.matches
    onUpdate?.(matches.value)
  }

  onMounted(() => {
    update()

    if ('addEventListener' in mediaQuery) {
      mediaQuery.addEventListener('change', update)
    } else {
      mediaQuery.addListener(update)
    }
  })

  onUnmounted(() => {
    if ('removeEventListener' in update) {
      mediaQuery.removeEventListener('change', update)
    } else {
      mediaQuery.removeListener(update)
    }
  })

  return matches
}
