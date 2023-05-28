import { tryOnBeforeMount, tryOnScopeDispose } from '@vueuse/core'
import { ref } from 'vue'

type MediaQuery =
  | '(prefers-reduced-motion: reduce)'
  | `(orientation: ${'landscape' | 'portrait'})`
  | `(prefers-color-scheme: ${'dark' | 'light'})`

export function useMediaQuery(
  query: MediaQuery,
  onUpdate?: (matches: boolean) => void
) {
  const mediaQuery = window.matchMedia(query)
  const matches = ref(false)

  const update = () => {
    matches.value = mediaQuery.matches
    onUpdate?.(matches.value)
  }

  tryOnBeforeMount(() => {
    update()

    if ('addEventListener' in mediaQuery) {
      mediaQuery.addEventListener('change', update)
    } else {
      ;(mediaQuery as MediaQueryList).addListener(update)
    }
  })

  tryOnScopeDispose(() => {
    if ('removeEventListener' in update) {
      mediaQuery.removeEventListener('change', update)
    } else {
      mediaQuery.removeListener(update)
    }
  })

  return matches
}
