<script setup lang="ts">
import { ref } from 'vue'
import { useMediaQuery } from '~/domain'

const vh = ref(`${window.innerHeight * 0.01}px`)

useMediaQuery('(orientation: landscape)', () => {
  vh.value = `${window.innerHeight * 0.01}px`
})
</script>

<template>
  <main
    :class="[
      $route.name === 'home'
        ? 'h-screen-save grid-area-[header/header/main/main]'
        : 'pt-12 pb-48 grid-area-[main] sm:pt-16 lg:pt-20 xl:pt-24'
    ]"
  >
    <RouterView></RouterView>
  </main>
</template>

<style scoped>
.h-screen-save {
  height: calc(v-bind(vh) * 100);
}
</style>
