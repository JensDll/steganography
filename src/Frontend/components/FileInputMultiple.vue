<script setup lang="ts">
interface Props {
  modelValue: File[]
  id?: string
}

const props = defineProps<Props>()

const emit = defineEmits({
  'update:modelValue': (files: File[]) => Array.isArray(files),
})

const files = computed<File[]>({
  get() {
    return props.modelValue
  },
  set(files) {
    emit('update:modelValue', files)
  },
})

const listeners = {
  change(e: EventWithTarget<HTMLInputElement>) {
    const input = e.target
    const fileList = input.files

    if (fileList) {
      const fileListItems: File[] = []

      for (let i = 0; i < fileList.length; i++) {
        fileListItems.push(fileList[i])
      }

      files.value = fileListItems
    }
  },
}
</script>

<template>
  <div
    class="file-input relative flex flex-col items-center justify-center border-2 border-dashed"
  >
    <input :id="id" type="file" multiple v-on="listeners" />
    <slot>Choose files or drag and drop here</slot>
  </div>
</template>

<style scoped></style>
