<script setup lang="ts">
import { onMounted, ref, onUnmounted, type Ref } from 'vue'
import { gsap } from 'gsap'

import { useMediaQuery, type AnimationHooks } from '~/domain'

const emit = defineEmits(['click'])

const containerRef = ref() as Ref<HTMLDivElement>
const lockDim = 24
const numLocks = ref(8)
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
    class="relative grid h-28 place-items-center overflow-x-hidden"
  >
    <TransitionGroup appear v-on="animation">
      <div
        v-for="i in numLocks"
        :key="`closed-${i}`"
        class="i-heroicons-solid-lock-open absolute top-0 right-full origin-center text-decode-200 dark:text-decode-500"
      ></div>
      <div
        v-for="i in numLocks"
        :key="`closed-${i}`"
        class="i-heroicons-solid-lock-closed absolute top-0 right-full text-encode-200 dark:text-encode-500"
      ></div>
    </TransitionGroup>
    <AppButton variant="landing" @click="emit('click')">
      Getting Started
    </AppButton>
  </div>
</template>
