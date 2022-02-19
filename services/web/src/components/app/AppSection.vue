<script setup lang="ts"></script>

<template>
  <section class="section">
    <slot></slot>
  </section>
</template>

<style scoped>
.section {
  overflow: clip;
  width: 100%;
  box-shadow: 0 6px 30px 0 rgba(0, 0, 0, 0.1);
}

@media (min-width: 1430px) {
  .section {
    @apply mx-auto max-w-7xl rounded-2xl;
    box-shadow: 8px 8px 40px 0 rgba(0, 0, 0, 0.1);
  }
}
</style>
