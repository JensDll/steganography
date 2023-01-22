<script setup lang="ts">
defineProps({
  loading: {
    type: Boolean,
    default: false
  }
})
</script>

<template>
  <Transition name="loading">
    <div
      v-if="loading"
      class="spin i-custom-loading firefox-border-animation-fix"
    ></div>
  </Transition>
</template>

<style scoped>
.spin {
  animation: spin 900ms linear infinite;
}

@keyframes spin {
  to {
    transform: rotate(360deg);
  }
}

.loading-enter-active,
.loading-leave-active {
  transition: opacity 200ms;
}

.loading-enter-from,
.loading-leave-to {
  opacity: 0;
}
</style>
