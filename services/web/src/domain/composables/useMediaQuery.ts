import { onMounted, onUnmounted, ref } from 'vue'

export function useMediaQuery(query: string) {
  const mediaQuery = window.matchMedia(query)
  const matches = ref(false)

  const update = () => {
    matches.value = mediaQuery.matches
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
