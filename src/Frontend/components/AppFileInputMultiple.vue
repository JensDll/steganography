<script setup lang="ts">
interface Props {
  modelValue: File[]
  id?: string
  accept?: string
}

const props = defineProps<Props>()

const emit = defineEmits({
  'update:modelValue': (files: File[]) => Array.isArray(files),
})

const { listeners } = useVModelFiles(props, emit)
</script>

<template>
  <div
    class="file-input relative flex flex-col items-center justify-center border-2 border-dashed"
  >
    <input type="file" multiple v-on="listeners" :id="id" :accept="accept" />
    <slot></slot>
  </div>
</template>

<style scoped></style>
