<script setup lang="ts">
import { type PropType } from 'vue'

import { useVModelFiles } from '~/domain'

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
</script>

<template>
  <div v-if="!multiple">
    <label v-if="label" for="file-input">{{ label }}</label>
    <div class="flex items-center">
      <label class="mb-0 mr-4" for="file-input">
        <HeroiconsSolid:paperClip class="h-6 w-6" />
      </label>
      <div
        class="custom-file-input relative mr-4 cursor-pointer p-3"
        :class="{ error: errors.length }"
      >
        <input
          id="file-input"
          type="file"
          class="absolute inset-0 h-full w-full opacity-0"
          v-on="fileListeners"
        />
        <span>
          <template v-if="files.length">
            {{ files[0].name }}
          </template>
          <template v-else>Choose or drag and drop here</template>
        </span>
      </div>
      <AppImagePreview
        :src="files[0]"
        removable
        title="Remove attachment"
        @remove="removeFile(0)"
      />
    </div>
    <FormErrors :errors="errors" />
  </div>
  <div v-else>
    <label v-if="label" :for="`file-${label}`">{{ label }}</label>
    <div class="group relative grid place-items-center py-6 px-10">
      <input
        :id="`file-${label}`"
        class="absolute inset-0 cursor-pointer opacity-0"
        type="file"
        :accept="accept"
        multiple
        v-on="fileListeners"
      />
      <AppIcon
        icon="ImagePlus"
        class="h-12 w-12 text-slate-400 group-hover:text-slate-500"
      />
      <div class="text-center">
        <p>
          <span
            :class="['font-semibold text-sky-600 group-hover:text-sky-700']"
          >
            Upload a file
          </span>
          or drag and drop
        </p>
      </div>
    </div>
    <FormErrors :errors="errors" class="mt-1" />
    <ul v-if="files.length" class="mt-2">
      <li
        v-for="(file, i) in files"
        :key="file.name"
        class="group flex cursor-pointer items-center"
        @click="removeFile(i)"
      >
        <Ic:twotoneRemoveCircle
          class="mr-2 h-6 w-6 text-red-500 group-hover:text-red-700"
        />
        <span class="group-hover:line-through">{{ file.name }}</span>
      </li>
    </ul>
  </div>
</template>

<style scoped></style>
