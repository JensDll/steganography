import { gsap } from 'gsap'

import type { AnimationHooks } from '~/common/types'

export const animateAppear: AnimationHooks = {
  enter(el, done) {
    gsap.from(el, {
      y: -4,
      opacity: 0,
      duration: 0.1,
      onComplete: done
    })
  },
  leave(el, done) {
    gsap.to(el, {
      y: -4,
      opacity: 0,
      duration: 0.1,
      onComplete: done
    })
  }
}

export function animateShake(target: gsap.TweenTarget) {
  const timeline = gsap.timeline()

  timeline.to(target, {
    x: -8,
    duration: 0.1
  })

  timeline.to(target, { x: 0, ease: 'elastic.out(1.2, 0.3)', duration: 0.6 })
}
