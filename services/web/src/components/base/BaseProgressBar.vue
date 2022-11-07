<script setup lang="ts">
import { gsap } from 'gsap'
import { type PropType, onMounted, ref, watch, type Ref } from 'vue'

import type { AnimationHooks } from '~/common/types'

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
const progressBarRef = ref() as Ref<HTMLDivElement>

const loadingHooks: AnimationHooks = {
  enter(el, done) {
    gsap.to(el, {
      duration: 0.65,
      rotate: 360,
      ease: 'power1.out',
      repeat: -1
    })
    gsap.from(el, {
      duration: 0.4,
      opacity: 0,
      onComplete: done
    })
  },
  leave(el, done) {
    gsap.to(el, {
      duration: 0.2,
      opacity: 0,
      x: -6,
      onComplete: done
    })
  }
}

let barTween: gsap.core.Tween

function barReset() {
  gsap.set(progressBarRef.value, {
    x: '-100%',
    immediateRender: true
  })
  barTween?.time(0).kill()
}

function barAnimate() {
  barTween = gsap.to(progressBarRef.value, {
    duration: 2,
    ease: 'none',
    x: 0,
    onUpdate() {
      const progressPercentage = (this.progress() * 100) | 0
      progress.value = progressPercentage

      if (props.active) {
        if (progressPercentage > 84) {
          this.pause()
        } else if (progressPercentage > 83) {
          isLongLoad.value = true
          emit('longLoad')
        } else if (progressPercentage > 75) {
          this.timeScale(0.1)
        } else if (progressPercentage > 60) {
          this.timeScale(0.15)
        } else if (progressPercentage > 50) {
          this.timeScale(0.2)
        } else if (progressPercentage > 40) {
          this.timeScale(0.3)
        } else if (progressPercentage > 20) {
          this.timeScale(0.5)
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
    class="grid min-h-[1.5rem] grid-cols-[2.5rem_1fr_2.5rem] items-center opacity-0"
    :class="{ 'opacity-100': isActive }"
  >
    <div class="text-sm">{{ progress }}</div>
    <div
      class="relative h-2 overflow-hidden rounded-full bg-white/80 safari-fix-overflow"
    >
      <div
        ref="progressBarRef"
        class="absolute inset-x-0 -inset-y-[1px] rounded-full bg-highlight"
        :class="
          {
            encode: 'bg-encode-500 dark:bg-encode-600',
            decode: 'bg-decode-500 dark:bg-decode-600'
          }[variant]
        "
      ></div>
    </div>
    <Transition v-on="loadingHooks">
      <div
        v-if="isLongLoad"
        class="i-custom-loading justify-self-end text-highlight firefox-border-animation-bug-fix"
      ></div>
    </Transition>
  </div>
</template>
