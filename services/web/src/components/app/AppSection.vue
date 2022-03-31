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
  box-shadow: 0 2px 20px 0 rgb(theme('colors.rgb.gray.900') / 0.12);
}

.dark .section {
  box-shadow: 0 2px 20px 0 rgb(theme('colors.rgb.gray.700') / 0.8);
}

@screen section {
  .section {
    @apply mx-auto max-w-7xl rounded-2xl;
    box-shadow: 4px 4px 40px 0 rgb(theme('colors.rgb.gray.900') / 0.12);
  }

  .dark .section {
    box-shadow: 4px 4px 40px 0 rgb(theme('colors.rgb.gray.700') / 0.4);
  }
}
</style>
