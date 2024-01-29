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

const { files, remove, listeners } = useVModelFiles(props, emit)

const filename = computed(() => {
  const filename = files.value[0].name
  return filename.length > 26
    ? [filename.substring(0, 12), filename.substring(filename.length - 12)]
    : [filename]
})
</script>

<template>
  <div
    class="file-input relative border-2 border-dashed px-8 py-4 text-center md:max-w-md md:py-6"
  >
    <div class="h-10 leading-10">
      <span
        v-if="files.length"
        @click="remove(0)"
        class="group relative z-20 cursor-pointer hover:text-gray-500 hover:line-through"
        title="Remove file"
      >
        <AppFilePreview
          :file="files[0]"
          class="inline-block h-full -translate-y-0.5 rounded-full group-hover:opacity-80"
        />
        {{ filename.join('&hellip;') }}
      </span>
      <slot v-else></slot>
    </div>
    <input type="file" v-on="listeners" :id="id" :accept="accept" />
  </div>
</template>

<style scoped></style>
