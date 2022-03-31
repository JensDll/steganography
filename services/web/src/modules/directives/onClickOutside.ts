import type { Directive } from 'vue'

interface DirectiveElement extends Element {
  __directive_onUnmount: () => void
}

export const onClickOutside: Directive<DirectiveElement> = {
  mounted(el, binding) {
    const windowListener = (e: PointerEvent) => {
      if (el.contains(e.target as Node)) {
        return
      }

      binding.value()
    }

    window.addEventListener('pointerup', windowListener)

    el.__directive_onUnmount = function () {
      window.removeEventListener('pointerup', windowListener)
    }
  },
  unmounted(el) {
    el.__directive_onUnmount()
  }
}
