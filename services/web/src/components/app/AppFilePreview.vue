<script setup lang="ts">
import { watch, computed, ref, type PropType } from 'vue'
import { guards, useFileSize } from '~/domain'

const emit = defineEmits(['remove'])

const props = defineProps({
  file: {
    type: File as PropType<File | undefined>,
    default: undefined
  },
  alt: {
    type: String,
    default: 'image'
  },
  variant: {
    type: String as PropType<'default' | 'reduced'>,
    default: 'default'
  }
})

const loading = ref(false)
const loaded = ref(false)

const isImage = computed(() => props.file?.type.startsWith('image/'))

const src = computed(() => {
  if (props.file) {
    console.log()
    if (src.value) {
      URL.revokeObjectURL(src.value)
    }
    return URL.createObjectURL(props.file)
  }

  return undefined
})

const fileSize = useFileSize(props)

watch(
  () => props.file,
  file => {
    if (guards.isImage(file)) {
      loading.value = true
    } else {
      loaded.value = false
    }
  },
  {
    immediate: true
  }
)

function handleClick() {
  loaded.value = false
  loading.value = false
  emit('remove')
}

function handleLoad() {
  loaded.value = true
  loading.value = false
}
</script>

<template>
  <div
    :class="[
      'group',
      { 'cursor-pointer': !isImage || loaded },
      {
        default: 'flex flex-col items-center justify-center rounded-lg ',
        reduced: 'relative center-children'
      }[variant]
    ]"
    @click="handleClick"
  >
    <div
      v-if="isImage"
      v-show="loaded || loading"
      :class="[
        'h-12 w-12 overflow-clip rounded-full shadow center-children',
        { 'group-hover:opacity-30': loaded }
      ]"
    >
      <img
        v-if="src"
        :src="src"
        :alt="alt"
        class="min-h-full min-w-full"
        @load="handleLoad"
      />
      <div
        :class="[
          'h-6 w-6',
          {
            'group-hover:bg-heroicons-outline-trash-black dark:group-hover:bg-heroicons-outline-trash-white':
              loaded
          }
        ]"
      ></div>
    </div>
    <p
      v-if="variant === 'default'"
      class="mt-1 break-all text-sm group-hover:line-through"
    >
      {{ file?.name }}
    </p>
    <p
      :class="[
        'whitespace-nowrap pt-1 text-xs text-gray-600 dark:text-gray-300',
        { 'group-hover:line-through': !isImage || loaded },
        {
          default: '',
          reduced: 'absolute top-full pt-2'
        }[variant]
      ]"
    >
      {{ fileSize }}
    </p>
  </div>
</template>
