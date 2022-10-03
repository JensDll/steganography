<script setup lang="ts">
defineEmits(['submit'])
</script>

<template>
  <section class="section">
    <slot></slot>
  </section>
</template>

<style scoped>
.section {
  overflow: hidden;
  width: 100%;
  box-shadow: 0 2px 20px 0 theme('colors.gray.900' / 0.12);
}

.dark .section {
  box-shadow: 0 2px 20px 0 theme('colors.black' / 0.75);
}

@media (min-width: 1340px) {
  .section {
    @apply mx-auto max-w-6xl rounded-2xl;
    width: 90%;
    box-shadow: 4px 4px 40px 0 theme('colors.gray.900' / 0.12);
  }

  .dark .section {
    box-shadow: 4px 4px 40px 0 theme('colors.black' / 0.75);
  }
}
</style>
