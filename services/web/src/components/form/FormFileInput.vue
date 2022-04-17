<script setup lang="ts">
import type { PropType } from 'vue'

import { useFileSize, useVModelFiles } from '~/domain'

const emit = defineEmits({
  'update:modelValue': (files: File[]) => Array.isArray(files)
})

const props = defineProps({
  label: {
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
        files.length
          ? 'card-grid p-6'
          : 'flex flex-col items-center justify-center px-6'
      ]"
    >
      <input
        :id="`file-${label}`"
        class="absolute inset-0 h-full w-full cursor-pointer opacity-0"
        type="file"
        :accept="accept"
        multiple
        v-on="fileListeners"
      />
      <AppFilePreview
        v-for="(file, i) in files"
        :key="file.name"
        :file="file"
        class="relative"
        @remove="removeFile(i)"
      />
      <template v-if="!files.length">
        <div class="h-5 w-5 i-heroicons-solid-paper-clip"></div>
        <p class="text-center">
          <span
            class="font-semibold text-border-form-highlight"
            :class="{ error: errors.length }"
          >
            Choose files
          </span>
          <span class="hidden md:inline-block">or drag and drop</span>
          here
        </p>
      </template>
    </div>
    <p class="mt-2">{{ totalFileSize }}</p>
    <FormErrors :errors="errors" />
  </div>
  <div v-else>
    <label v-if="label" for="file-input">{{ label }}</label>
    <div class="flex items-center">
      <div
        class="custom-file-input relative flex cursor-pointer items-center py-6 px-8"
        :class="{
          error: errors.length,
          'mr-4': files[0]
        }"
      >
        <input
          id="file-input"
          type="file"
          class="absolute inset-0 h-full w-full opacity-0"
          :accept="accept"
          v-on="fileListeners"
        />
        <div
          v-if="!files.length"
          class="mr-2 h-5 w-5 i-heroicons-solid-paper-clip"
        ></div>
        <div class="text-center">
          <template v-if="files.length">
            {{ files[0].name }}
          </template>
          <template v-else>
            <span
              class="font-semibold text-border-form-highlight"
              :class="{ error: errors.length }"
            >
              Choose
            </span>
            <span class="hidden md:inline-block">or drag and drop</span> here
          </template>
        </div>
      </div>
      <AppFilePreview
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
