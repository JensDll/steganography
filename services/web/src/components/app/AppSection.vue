<script setup lang="ts"></script>

<template>
  <section class="section">
    <slot></slot>
  </section>
</template>

<style scoped>
.section {
  overflow: hidden;
  width: 100%;
  box-shadow: 0 6px 30px 0 rgba(theme('colors.rgb.gray.900') / 0.12);
}

.dark .section {
  box-shadow: 0 6px 30px 0 rgba(theme('colors.rgb.gray.50') / 0.12);
}

@media (min-width: 1430px) {
  .section {
    @apply mx-auto max-w-7xl rounded-2xl;
    box-shadow: 0px 4px 40px 0 rgba(theme('colors.rgb.gray.900') / 0.12);
  }

  .dark .section {
    box-shadow: 0 4px 40px 0 rgba(theme('colors.rgb.gray.50') / 0.12);
  }
}
</style>
