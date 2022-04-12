<script setup lang="ts"></script>

<template>
  <main
    :class="[
      $route.name === 'home'
        ? 'h-screen grid-area-[header/header/main/main]'
        : 'pt-12 pb-48 grid-area-[main] sm:pt-16 lg:pt-20 xl:pt-24'
    ]"
  >
    <RouterView></RouterView>
  </main>
</template>
