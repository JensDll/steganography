<script setup lang="ts">
import CallToAction from './CallToAction.vue'
</script>

<template>
  <section class="bg-gradient grid h-screen">
    <div
      class="grid grid-rows-[auto_auto_1fr] justify-items-center self-center"
    >
      <div class="px-4 lg:px-6">
        <h1 class="text-center">Image Data Hiding</h1>
        <p class="mt-6 text-center text-lg">Embed secret messages in images</p>
      </div>
      <CallToAction
      @click="$router.push({ name: 'encode' })"
      
        class="mt-6 w-full max-w-4xl px-4 lg:px-6"
        
      />
    </div>
  </section>
</template>

<style scoped>
.bg-gradient {
  background-image: linear-gradient(
      20deg,
      theme('colors.sky.100'),
      rgba(0, 0, 0, 0) 50%
    ),
    linear-gradient(-20deg, theme('colors.emerald.100'), rgba(0, 0, 0, 0) 50%);
}
</style>
