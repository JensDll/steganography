<script setup lang="ts">
import { ref } from 'vue'

const isNavMobileOpen = ref(false)
</script>

<template>
  <header
    class="bg-bg-base supports-backdrop-blur:bg-bg-base/95 sticky top-0 z-50 border-b backdrop-blur grid-area-[header]"
    :class="{ '!border-b-0': $route.name === 'home' }"
  >
    <div class="container flex items-center justify-between py-6 md:items-end">
      <TheLogo
        class="h-7 cursor-pointer"
        @click="$router.push({ name: 'home' })"
      />
      <TheNav class="hidden md:block lg:text-sm" />
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
