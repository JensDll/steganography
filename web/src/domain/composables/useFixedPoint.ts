import { ComponentPublicInstance, onMounted } from 'vue'

export function useFixedPoint(x: any, y: any) {
  let target: ComponentPublicInstance | Element | null

  const functionRef = (el: Element | ComponentPublicInstance | null) => {
    target = el
  }

  onMounted(() => {})

  return { target: functionRef }
}
