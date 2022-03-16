import type { Directive } from 'vue'

interface DirectiveElement extends Element {
  __directive_onUnmount: () => void
}

export const onClickOutside: Directive<DirectiveElement> = {
  mounted(el, binding) {
    let hasDownUpTriggeredOnTarget = false

    const clickCaptureListener = () => {
      if (hasDownUpTriggeredOnTarget) {
        hasDownUpTriggeredOnTarget = false
        return
      }

      binding.value()
    }

    const downUpListener = () => {
      hasDownUpTriggeredOnTarget = true
    }

    el.addEventListener('pointerdown', downUpListener)
    el.addEventListener('pointerup', downUpListener)

    window.addEventListener('click', clickCaptureListener, {
      capture: true
    })

    el.__directive_onUnmount = function () {
      this.removeEventListener('pointerdown', downUpListener)
      this.removeEventListener('pointerup', downUpListener)
      window.removeEventListener('click', clickCaptureListener, {
        capture: true
      })
    }
  },
  unmounted(el) {
    el.__directive_onUnmount()
  }
}
