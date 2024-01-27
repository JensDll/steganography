<script setup lang="ts"></script>

<template>
  <nav
    class="fixed bottom-0 grid grid-flow-col py-4 container md:static md:block md:space-x-4 md:pt-10"
  >
    <NuxtLink to="/encode" class="text-center">Encode</NuxtLink>
    <NuxtLink to="/decode" class="text-center">Decode</NuxtLink>
  </nav>
  <NuxtPage />
</template>

<style scoped></style>
