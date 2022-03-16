<script setup lang="ts"></script>

<template>
  <main
    :class="[
      'pb-32 lg:pb-40',
      $route.name === 'home'
        ? 'grid-area-[header/header/main/main]'
        : 'pt-12 grid-area-[main] sm:pt-16 lg:pt-20 xl:pt-24'
    ]"
  >
    <RouterView></RouterView>
  </main>
</template>

<style scoped></style>
