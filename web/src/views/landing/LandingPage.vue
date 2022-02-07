<script setup lang="ts">
import AnimatedLocks from './AnimatedLocks.vue'
// import { onBeforeUnmount, onMounted, ref, Ref } from 'vue'

// import EncodeToggle from './EncodeToggle.vue'

// const formContainer = ref() as Ref<HTMLElement>
// const observer = new ResizeObserver(([entry]) => {
//   const target = entry.target as HTMLElement
//   target.style.setProperty(
//     '--form-container-width',
//     `${entry.borderBoxSize[0].inlineSize}px`
//   )
// })

// onMounted(() => {
//   observer.observe(formContainer.value)
// })

// onBeforeUnmount(() => {
//   observer.disconnect()
// })
</script>

<template>
  <section class="h-screen bg-slate-50">
    <div class="px-4 pt-[20vh] md:container md:mx-auto lg:px-6">
      <h1 class="text-center">Image Data Hiding</h1>
      <p class="mt-6 text-center text-lg">Embed secret messages in images</p>
    </div>
    <animated-locks class="mt-8 max-w-screen-lg md:mx-auto" />
  </section>
  <section class="h-screen bg-emerald-50"></section>
  <!-- <section class="h-[100vh] bg-slate-50">
      <div
        class="main relative mx-auto overflow-x-hidden md:container"
        ref="formContainer"
      >
        <encode-toggle class="encode-toggle mt-3" />
        <div class="fade-left"></div>
        <div class="fade-right"></div>
        <router-view v-slot="{ Component, route }">
          <transition :name="route.meta.transition">
            <component :is="Component" />
          </transition>
        </router-view>
      </div>
    </section> -->
</template>

<style lang="postcss" scoped>
.landing {
  --px: 1rem;

  & .wrapper {
    padding: 0 var(--px);
  }
}

.fade-left,
.fade-right {
  @apply absolute inset-y-0 z-10;
  width: var(--px);
}

.fade-left {
  left: 0;
  background: linear-gradient(
    to right,
    theme('colors.slate.50'),
    rgba(255, 255, 255, 0)
  );
}

.fade-right {
  right: 0;
  background: linear-gradient(
    to left,
    theme('colors.slate.50'),
    rgba(255, 255, 255, 0)
  );
}

.slide-left-leave-active,
.slide-left-enter-active,
.slide-right-leave-active,
.slide-right-enter-active {
  @apply transition-transform duration-300;
}

.slide-left-enter-active,
.slide-right-enter-active {
  transition-delay: 150ms;
}

.slide-left-enter-from {
  transform: translateX(calc(var(--form-container-width)));
}

.slide-left-leave-to {
  transform: translateX(calc(-1 * var(--form-container-width)));
}

.slide-right-enter-from {
  transform: translateX(calc(-1 * var(--form-container-width)));
}

.slide-right-leave-to {
  transform: translateX(calc(var(--form-container-width)));
}

@screen sm {
  .landing {
    --px: 2rem;
  }
}
</style>
