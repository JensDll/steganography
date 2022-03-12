import type { Directive } from 'vue'

interface DirectiveElement extends Element {
  onUnmount: () => void
}

export const onClickOutside: Directive<DirectiveElement> = {
  mounted(el, binding) {
    const clickCaptureListener = (event: MouseEvent) => {
      if (el === event.target || el.contains(event.target as Node)) {
        return
      }

      binding.value()
    }

    window.addEventListener('click', clickCaptureListener, {
      capture: true
    })

    el.onUnmount = () => {
      window.removeEventListener('click', clickCaptureListener, {
        capture: true
      })
    }
  },
  unmounted(el) {
    el.onUnmount()
  }
}
