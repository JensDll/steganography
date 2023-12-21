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

const filename = computed(() => {
  const file = files.value[0]
  const filename = file.name
  const filenameLength = filename.length

  return filenameLength > 26
    ? [filename.substring(0, 12), filename.substring(filenameLength - 12)]
    : [filename]
})
</script>

<template>
  <section
    class="flex flex-col space-y-2 md:flex-row md:items-center md:space-x-4 md:space-y-0"
  >
    <div
      class="file-input relative flex flex-col items-center justify-center border-2 border-dashed px-8 py-4 md:py-6"
    >
      <input :id="id" type="file" v-on="listeners" />
      <span>
        <slot>
          Choose
          <span class="hidden md:inline-block">or drag and drop</span>
          here
        </slot>
      </span>
    </div>
    <div v-if="files.length" class="text-right">
      {{ filename.join('&hellip;') }}
    </div>
    <div v-else class="text-right">No file selected</div>
  </section>
</template>

<style scoped></style>
