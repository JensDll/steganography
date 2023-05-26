<script setup lang="ts"></script>

<template>
  <main
    id="the-main"
    :class="[
      'relative',
      $route.name === 'home'
        ? 'h-screen grid-area-[header/header/main/main]'
        : 'pb-32 pt-12 grid-area-[main] sm:pb-36 sm:pt-14 lg:pb-40 lg:pt-16 xl:pb-52 xl:pt-20'
    ]"
  >
    <RouterView></RouterView>
  </main>
</template>
