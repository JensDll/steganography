<script setup lang="ts"></script>

<template>
  <main
    id="the-main"
    :class="[
      'relative',
      $route.name === 'home'
        ? 'h-screen grid-area-[header/header/main/main]'
        : 'pt-12 pb-32 grid-area-[main] sm:pt-16 sm:pb-36 lg:pt-20 lg:pb-40 xl:pt-24 xl:pb-52'
    ]"
  >
    <RouterView></RouterView>
  </main>
</template>
