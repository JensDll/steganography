import { gsap } from 'gsap'

import type { AnimationHooks } from '../common/types'

const appear: AnimationHooks = {
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

function shake(target: gsap.TweenTarget) {
  const tl = gsap.timeline()
  tl.to(target, {
    x: -8,
    duration: 0.1
  })
  tl.to(target, { x: 0, ease: 'elastic.out(1.2, 0.3)', duration: 0.6 })
}

export const animation = {
  appear,
  shake
} as const
