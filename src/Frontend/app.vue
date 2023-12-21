<script setup></script>

<template>
  <nav class="fixed bottom-0 grid grid-flow-col bg-slate-50 container">
    <NuxtLink to="/encode" class="link">
      <div class="i-heroicons-lock-closed"></div>
    </NuxtLink>
    <NuxtLink to="/decode" class="link">
      <div class="i-heroicons-lock-open"></div>
    </NuxtLink>
  </nav>
  <NuxtPage />
</template>

<style scoped>
.link {
  @apply flex items-center justify-center py-3  underline;
}
</style>
