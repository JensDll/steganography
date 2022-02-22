<script setup lang="ts">
import { type PropType } from 'vue'

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
    class="mt-1 text-sm text-[color:var(--form-error-text-color)]"
  >
    <li v-for="error in errors" :key="error">{{ error }}</li>
  </ul>
</template>
