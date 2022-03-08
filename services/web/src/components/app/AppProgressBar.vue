<script setup lang="ts">
import { onMounted, ref, watch } from 'vue'
import { gsap } from 'gsap'

import { type AnimationHooks } from '~/domain'

const emit = defineEmits(['longLoad'])

const props = defineProps({
  active: {
    type: Boolean
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
      isActive.value = false
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
    <div class="relative h-2 overflow-hidden rounded-full bg-white shadow-sm">
      <div id="bar" class="absolute inset-0 rounded-full bg-encode-500"></div>
    </div>
    <Transition v-on="loadingHooks">
      <AppIcon
        v-if="isLongLoad"
        icon="LoadingCircle"
        class="justify-self-end text-encode-500"
      />
    </Transition>
  </div>
</template>
