<script setup lang="ts">
import { defineComponent, computed, PropType } from 'vue'

type FileHelper = {
  src: string
  file: File
}

const emit = defineEmits({
  'update:modelValue': (files: File[]) => true
})

const props = defineProps({
  label: {
    type: String
  },
  modelValue: {
    type: Array as PropType<File[]>,
    required: true
  },
  errors: {
    type: Array as PropType<string[]>,
    default: () => []
  },
  image: {
    type: Boolean
  },
  multiple: {
    type: Boolean
  },
  accept: {
    type: String
  }
})

const hasError = computed(() => props.errors.length > 0)
const files = computed<File[]>({
  get(): File[] {
    return props.modelValue
  },
  set(files) {
    emit('update:modelValue', files)
  }
})
const fileHelpers = computed<FileHelper[]>(() =>
  files.value.map((file) => ({
    src: URL.createObjectURL(file),
    file
  }))
)
const isFileSelected = computed<boolean>(() => fileHelpers.value.length > 0)

const removeFile = (i: number) => {
  URL.revokeObjectURL(fileHelpers.value[i].src)
  files.value.splice(i, 1)
}

const unique = (files: File[]) => {
  const lookup = new Set<string>()
  return files.filter((f) => {
    const keep = !lookup.has(f.name)
    lookup.add(f.name)
    return keep
  })
}

const handleChange = (e: Event) => {
  const input = e.target as HTMLInputElement
  const fileList = input.files
  if (fileList) {
    for (let i = 0; i < fileList.length; i++) {
      const file = fileList[i]
      files.value[i] = file
    }
    files.value = unique(files.value)
  }
  input.value = ''
}
</script>

<template>
  <div>
    <label v-if="label" class="label" :for="`file-${label}`">{{ label }}</label>
    <div
      :class="[
        'input border-2 group relative py-6 px-10 border-dashed grid place-items-center hover:bg-slate-50',
        { error: hasError }
      ]"
    >
      <input
        :id="`file-${label}`"
        class="w-full h-full absolute opacity-0 cursor-pointer"
        type="file"
        :accept="accept"
        :multiple="multiple"
        @change="handleChange"
      />
      <image-plus-icon
        class="w-12 h-12 text-slate-400 group-hover:text-slate-500"
      />
      <div class="text-center">
        <p>
          <span
            :class="[
              'font-semibold text-sky-500 group-hover:text-sky-600',
              { '!text-red-500 group-hover:!text-red-700': hasError }
            ]"
          >
            Upload a file
          </span>
          or drag and drop
        </p>
        <slot></slot>
        <div v-if="image && isFileSelected" class="flex flex-col items-center">
          <template v-for="{ file, src } in fileHelpers" :key="src">
            <img :src="src" class="w-32 h-32 mx-auto mb-2 mt-8" />
            <p>{{ file.name }}</p>
          </template>
        </div>
      </div>
    </div>
    <form-errors :errors="errors" class="mt-1" />
    <ul v-if="files.length" class="mt-2">
      <li
        v-for="(file, i) in files"
        class="flex items-center cursor-pointer group"
        :key="file.name"
        @click="removeFile(i)"
      >
        <ic:twotone-remove-circle
          class="mr-2 group-hover:text-red-700 text-red-500 w-6 h-6"
        />
        <span class="group-hover:line-through">{{ file.name }}</span>
      </li>
    </ul>
  </div>
</template>

<style lang="postcss" scoped></style>
