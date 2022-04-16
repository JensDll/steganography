<script setup lang="ts">
import { onMounted, ref, watch, type PropType } from 'vue'
import { gsap } from 'gsap'

import type { AnimationHooks } from '~/domain'

const emit = defineEmits(['longLoad'])

const props = defineProps({
  active: {
    type: Boolean
  },
  variant: {
    type: String as PropType<'encode' | 'decode'>,
    required: true
  }
})

const progress = ref(0)
const isActive = ref(props.active)
const isLongLoad = ref(false)

let barTween: gsap.core.Tween

function barReset() {
  gsap.set('#bar', {
    x: '-100%',
    immediateRender: true
  })
  barTween?.time(0).kill()
}

function barAnimate() {
  barTween = gsap.to('#bar', {
    duration: 1,
    ease: 'none',
    x: 0,
    onUpdate() {
      const progressPercentage = (this.progress() * 100) | 0
      progress.value = progressPercentage

      if (props.active) {
        if (progressPercentage > 89) {
          this.pause()
        } else if (progressPercentage > 88) {
          isLongLoad.value = true
          emit('longLoad')
        } else if (progressPercentage > 60) {
          this.timeScale(0.1)
        } else if (progressPercentage > 40) {
          this.timeScale(0.2)
        } else if (progressPercentage > 20) {
          this.timeScale(0.4)
        }
      }
    },
    onComplete() {
      isLongLoad.value = false
      setTimeout(() => {
        this.reverse()
      }, 200)
    },
    onReverseComplete() {
      if (!props.active) {
        isActive.value = false
      }
    }
  })
}

const loadingHooks: AnimationHooks = {
  enter(el, done) {
    gsap.to(el, {
      duration: 0.6,
      rotate: 360,
      ease: 'power1.out',
      repeat: -1
    })
    gsap.from(el, {
      duration: 0.2,
      opacity: 0,
      x: -5,
      onComplete: done
    })
  },
  leave(el, done) {
    gsap.to(el, {
      duration: 0.2,
      opacity: 0,
      x: -5,
      onComplete: done
    })
  }
}

onMounted(barReset)

watch(
  () => props.active,
  active => {
    if (active) {
      isActive.value = true
      barReset()
      barAnimate()
    } else if (barTween) {
      barTween.timeScale(6).resume()
    }
  }
)
</script>

<template>
  <div
    v-show="isActive"
    class="grid grid-cols-[2.5rem_1fr_2.5rem] items-center"
  >
    <div class="text-sm">{{ progress }}</div>
    <div
      class="relative h-2 overflow-hidden rounded-full bg-white shadow-sm dark:bg-gray-300"
    >
      <div
        id="bar"
        :class="[
          `absolute inset-0 rounded-full`,
          {
            encode: 'bg-encode-500 dark:bg-encode-600',
            decode: 'bg-decode-500 dark:bg-decode-600'
          }[variant]
        ]"
      ></div>
    </div>
    <Transition v-on="loadingHooks">
      <div
        v-if="isLongLoad"
        :class="[
          'h-6 w-6 justify-self-end i-custom-loading',
          {
            encode: 'text-encode-500',
            decode: 'text-decode-500'
          }[variant]
        ]"
      ></div>
    </Transition>
  </div>
</template>
