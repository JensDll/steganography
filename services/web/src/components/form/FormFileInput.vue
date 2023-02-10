<script setup lang="ts">
import type { PropType } from 'vue'

import { useFileSize, useVModelFiles } from '~/composables'

const emit = defineEmits({
  'update:modelValue': (files: File[]) => Array.isArray(files)
})

const props = defineProps({
  label: {
    type: String,
    default: ''
  },
  id: {
    type: String,
    default: ''
  },
  modelValue: {
    type: Array as PropType<File[]>,
    required: true
  },
  errors: {
    type: Array as PropType<string[]>,
    default: () => []
  },
  multiple: {
    type: Boolean
  },
  accept: {
    type: String,
    default: ''
  }
})

const { files, fileListeners, removeFile } = useVModelFiles(props, emit)

const totalFileSize = useFileSize(files)
</script>

<template>
  <div v-if="multiple">
    <label v-if="label" :for="`file-${label}`">{{ label }}</label>
    <div
      class="custom-file-input relative min-h-[8rem]"
      :class="[
        { error: errors.length },
        files.length ? 'card-grid p-6' : 'flex items-center justify-center px-6'
      ]"
    >
      <input
        :id="id || `file-${label}`"
        class="absolute inset-0 z-10 h-full w-full cursor-pointer opacity-0"
        type="file"
        :accept="accept"
        multiple
        v-on="fileListeners"
      />
      <BaseFilePreview
        v-for="(file, i) in files"
        :key="file.name"
        :file="file"
        class="z-20"
        @remove="removeFile(i)"
      />
      <div v-if="!files.length">
        <span
          class="i-heroicons-arrow-down-tray-20-solid mr-1 inline-block translate-y-[4.5px]"
        ></span>
        <span
          class="font-semibold text-highlight"
          :class="{ error: errors.length }"
        >
          Choose files
        </span>
        <span class="hidden md:inline-block">or drag and drop</span>
        here
      </div>
    </div>
    <div class="mt-2">{{ totalFileSize }}</div>
    <FormErrors :errors="errors" />
  </div>
  <div v-else>
    <label v-if="label" for="file-input">{{ label }}</label>
    <div class="flex items-center">
      <div
        class="custom-file-input relative cursor-pointer py-6 px-10 text-center"
        :class="{ error: errors.length }"
      >
        <input
          :id="id || 'file-input'"
          type="file"
          class="absolute inset-0 z-10 h-full w-full opacity-0"
          :accept="accept"
          v-on="fileListeners"
        />
        <span
          v-if="!files.length"
          class="i-heroicons-paper-clip-20-solid mr-1 inline-block translate-y-1"
        ></span>
        <template v-if="files.length">
          {{ files[0].name }}
        </template>
        <template v-else>
          <span
            class="font-semibold text-highlight"
            :class="{ error: errors.length }"
          >
            Choose
          </span>
          <span class="hidden md:inline-block">or drag and drop</span> here
        </template>
      </div>
      <BaseFilePreview
        class="ml-6 mr-2"
        :file="files[0]"
        variant="reduced"
        @remove="removeFile(0)"
      />
    </div>
    <FormErrors :errors="errors" />
  </div>
</template>

<style scoped>
.card-grid {
  display: grid;
  gap: 1rem;
  grid-template-columns: repeat(auto-fill, minmax(min(7.5rem, 100%), 1fr));
}
</style>
