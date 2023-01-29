<script setup lang="ts">
import { ref } from 'vue'

const isNavMobileOpen = ref(false)
</script>

<template>
  <header
    id="the-header"
    class="sticky top-0 z-50 bg-fill backdrop-blur grid-area-[header] supports-backdrop-blur:bg-fill/80"
  >
    <div class="container flex items-center justify-between py-6 md:items-end">
      <TheLogo
        class="h-7 cursor-pointer dark:opacity-90"
        @click="$router.push({ name: 'home' })"
      />
      <TheNav class="hidden md:block" />
      <div
        class="i-heroicons-ellipsis-vertical cursor-pointer md:hidden"
        @click="isNavMobileOpen = true"
      ></div>
    </div>
  </header>
  <TheNavMobile
    v-if="isNavMobileOpen"
    class="translate-y-1 md:hidden"
    @close="isNavMobileOpen = false"
  />
</template>
