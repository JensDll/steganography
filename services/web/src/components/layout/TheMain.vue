<script setup lang="ts"></script>

<template>
  <main
    class="text-foo"
    :class="[
      $route.name === 'home'
        ? 'h-screen grid-area-[header/header/main/main]'
        : 'pt-14 pb-28 grid-area-[main] sm:pt-16 sm:pb-32 lg:pt-20 lg:pb-40 xl:pt-24 xl:pb-48'
    ]"
  >
    <RouterView></RouterView>
  </main>
</template>
