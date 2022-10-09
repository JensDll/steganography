<script setup lang="ts">
import CallToAction from './CallToAction.vue'
</script>

<template>
  <section class="bg-gradient grid h-full min-h-[600px]">
    <div
      class="grid grid-rows-[auto_auto_1fr] justify-items-center self-center pt-6 md:p-0"
    >
      <h1
        class="container text-center text-4xl font-extrabold tracking-tight sm:text-5xl lg:text-6xl"
      >
        Image Data Hiding
      </h1>
      <p class="container my-8 max-w-3xl text-center text-lg">
        Hide encrypted text and binary data in cover images.
      </p>
      <CallToAction
        class="mx-auto w-full max-w-2xl"
        @click="$router.push({ name: 'encode' })"
      />
    </div>
  </section>
</template>

<style scoped>
.bg-gradient {
  background-image: linear-gradient(
      12deg,
      theme('colors.encode.50'),
      theme('colors.encode.50' / 0) 60%
    ),
    linear-gradient(
      -12deg,
      theme('colors.decode.200'),
      theme('colors.decode.200' / 0) 60%
    );
}

.dark .bg-gradient {
  background-image: linear-gradient(
      12deg,
      theme('colors.encode.900' / 0.9),
      theme('colors.encode.900' / 0) 60%
    ),
    linear-gradient(
      -12deg,
      theme('colors.decode.900'),
      theme('colors.decode.900' / 0) 60%
    );
}
</style>
