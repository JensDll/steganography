<script setup lang="ts">
import { onMounted, onBeforeUnmount, ref, Ref } from 'vue'
import { gsap } from 'gsap'

const numLocks = 10
const containerRef = ref() as Ref<HTMLDivElement>
const animationState = {
  x: 0,
  randomY: () => 0,
  randomDuration: gsap.utils.random(1, 3, true)
}

const observer = new ResizeObserver(([entry]) => {
  animationState.x = entry.borderBoxSize[0].inlineSize + 24
})

onMounted(() => {
  observer.observe(containerRef.value)

  const containerBox = containerRef.value.getBoundingClientRect()
  animationState.x = containerBox.width + 24
  animationState.randomY = gsap.utils.random(0, containerBox.height - 24, true)

  const animate = (lockId: string) => {
    gsap.set(lockId, {
      x: 0,
      y: animationState.randomY()
    })
    gsap.to(lockId, {
      x: animationState.x,
      duration: animationState.randomDuration(),
      ease: 'none',
      onCompleteParams: [lockId],
      onComplete: animate
    })
  }

  for (let i = 1; i <= numLocks; ++i) {
    animate(`#closed-${i}`)
    animate(`#open-${i}`)
  }
})

onBeforeUnmount(() => {
  observer.disconnect()
})
</script>

<template>
  <div
    class="relative grid h-24 place-items-center overflow-x-hidden px-4 lg:px-6"
    ref="containerRef"
  >
    <app-button class="z-10" type="landing">Getting Started</app-button>
    <lock-closed-icon
      class="absolute top-0 right-full h-6 w-6 origin-center text-sky-200"
      v-for="i in numLocks"
      :key="i"
      :id="`closed-${i}`"
    />
    <lock-open-icon
      class="absolute top-0 right-full h-6 w-6 text-emerald-200"
      v-for="i in numLocks"
      :key="i"
      :id="`open-${i}`"
    />
  </div>
</template>

<style lang="postcss" scoped></style>
