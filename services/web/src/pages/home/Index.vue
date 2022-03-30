<script setup lang="ts">
import CallToAction from './CallToAction.vue'
</script>

<template>
  <section class="bg-gradient max-w grid h-screen min-h-[700px]">
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
      10deg,
      theme('colors.encode.50'),
      rgb(theme('colors.rgb.encode.50') / 0) 60%
    ),
    linear-gradient(
      -10deg,
      theme('colors.decode.200'),
      rgb(theme('colors.rgb.decode.200') / 0) 60%
    );
}

.dark .bg-gradient {
  background-image: linear-gradient(
      10deg,
      rgb(theme('colors.rgb.encode.900') / 0.9),
      rgb(theme('colors.rgb.encode.900') / 0) 60%
    ),
    linear-gradient(
      -10deg,
      theme('colors.decode.900'),
      rgb(theme('colors.rgb.decode.900') / 0) 60%
    );
}
</style>
