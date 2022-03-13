<script setup lang="ts">
import type { PropType } from 'vue'

defineProps({
  errors: {
    type: Array as PropType<string[]>,
    required: true
  }
})
</script>

<template>
  <ul
    v-show="errors.length"
    class="test text-sm text-red-500 last:mt-1 dark:text-red-600"
  >
    <li v-for="error in errors" :key="error">{{ error }}</li>
  </ul>
</template>
