<script setup lang="ts">
import CallToAction from './CallToAction.vue'
</script>

<template>
  <section class="bg-gradient grid h-screen">
    <div
      class="grid grid-rows-[auto_auto_1fr] justify-items-center self-center md:-translate-y-8"
    >
      <div class="container">
        <h1 class="text-center">Image Data Hiding</h1>
        <p class="mt-6 text-center text-lg">Embed secret messages in images</p>
      </div>
      <CallToAction
        class="mt-6 w-full max-w-4xl"
        @click="$router.push({ name: 'encode' })"
      />
    </div>
  </section>
</template>

<style scoped>
.bg-gradient {
  background-image: linear-gradient(
      20deg,
      theme('colors.encode.50'),
      rgba(0, 0, 0, 0) 50%
    ),
    linear-gradient(-20deg, theme('colors.decode.100'), rgba(0, 0, 0, 0) 50%);
}

.dark .bg-gradient {
  background-image: linear-gradient(
      10deg,
      rgba(6, 78, 59, 0.6),
      rgba(0, 0, 0, 0) 60%
    ),
    linear-gradient(-10deg, rgba(30, 58, 138, 0.6), rgba(0, 0, 0, 0) 60%);
}
</style>
