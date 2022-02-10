<script setup lang="ts"></script>

<template>
  <main
    class="min-h-screen bg-slate-50 transition-all duration-500 grid-area-[header/header/main/main]"
  >
    <RouterView></RouterView>
  </main>
</template>

<style scoped>
.bg-gradient-sky-emerald {
  background-image: linear-gradient(
      20deg,
      theme('colors.sky.100'),
      rgba(0, 0, 0, 0) 50%
    ),
    linear-gradient(-20deg, theme('colors.emerald.100'), rgba(0, 0, 0, 0) 50%);
}

.bg-gradient-sky {
  background-image: linear-gradient(
      20deg,
      theme('colors.sky.100'),
      rgba(0, 0, 0, 0) 50%
    ),
    linear-gradient(-20deg, theme('colors.sky.100'), rgba(0, 0, 0, 0) 50%);
}

.bg-gradient-emerald {
  background-image: linear-gradient(
      20deg,
      theme('colors.emerald.100'),
      rgba(0, 0, 0, 0) 50%
    ),
    linear-gradient(-20deg, theme('colors.emerald.100'), rgba(0, 0, 0, 0) 50%);
}
</style>
