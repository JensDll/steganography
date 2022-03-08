<script setup lang="ts"></script>

<template>
  <main
    :class="[
      'bg-gray-50 pb-48',
      $route.name === 'home'
        ? 'grid-area-[header/header/main/main]'
        : 'pt-12 grid-area-[main] sm:pt-16 lg:pt-20 xl:pt-24 2xl:pt-28'
    ]"
  >
    <RouterView></RouterView>
  </main>
</template>

<style scoped></style>
