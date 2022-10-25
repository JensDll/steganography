<script setup lang="ts">
import { gsap } from 'gsap'
import { type Ref, onMounted, onUnmounted, ref } from 'vue'

import type { AnimationHooks } from '~/common/types'
import { useMediaQuery } from '~/composables'

const emit = defineEmits(['click'])

const containerRef = ref() as Ref<HTMLDivElement>
const lockDim = 20
const numLocks = ref(6)
const animationState = {
  x: 0,
  randomY: () => 0,
  randomDuration: gsap.utils.random(1.5, 4, true)
}

const isReducedMotion = useMediaQuery('(prefers-reduced-motion: reduce)')

const animate = (icon: SVGElement) => {
  gsap.set(icon, {
    x: 0,
    y: animationState.randomY()
  })

  gsap.to(icon, {
    x: animationState.x,
    duration: animationState.randomDuration(),
    ease: 'none',
    onCompleteParams: [icon],
    onComplete: animate
  })
}

const animation: AnimationHooks<SVGElement> = {
  afterAppear(icon) {
    const duration = animationState.randomDuration()

    gsap.set(icon, {
      x: isReducedMotion.value ? lockDim : 0,
      y: animationState.randomY()
    })

    const tween = gsap
      .to(icon, {
        x: animationState.x,
        duration,
        ease: 'none',
        onCompleteParams: [icon],
        onComplete: animate
      })
      .pause(gsap.utils.random(0, duration))

    if (!isReducedMotion.value) {
      tween.resume()
    }
  }
}

const observer = new ResizeObserver(([entry]) => {
  animationState.x = entry.contentRect.width
  !isReducedMotion.value && (animationState.x += lockDim)
  animationState.randomY = gsap.utils.random(
    0,
    entry.contentRect.height - lockDim,
    true
  )
})

onMounted(() => {
  observer.observe(containerRef.value)
})

onUnmounted(() => {
  observer.disconnect()
})
</script>

<template>
  <div
    ref="containerRef"
    class="relative grid h-28 place-items-center overflow-hidden"
  >
    <TransitionGroup appear v-on="animation">
      <div
        v-for="i in numLocks"
        :key="`closed-${i}`"
        class="i-heroicons-lock-open-20-solid absolute top-0 right-full text-decode-200 dark:text-decode-500"
      ></div>
      <div
        v-for="i in numLocks"
        :key="`closed-${i}`"
        class="i-heroicons-lock-closed-20-solid absolute top-0 right-full text-encode-200 dark:text-encode-500"
      ></div>
    </TransitionGroup>
    <BaseButton variant="landing" @click="emit('click')">
      Getting Started
    </BaseButton>
  </div>
</template>
