<script setup lang="ts">
import { watch, computed, ref, type PropType } from 'vue'
import { useFileSize } from '~/domain'

const emit = defineEmits(['remove'])

const props = defineProps({
  file: {
    type: Object as PropType<File | undefined>,
    default: undefined
  },
  alt: {
    type: String,
    default: 'image'
  },
  type: {
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
    if (file) {
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
    class="group"
    :class="[
      { 'cursor-pointer': !isImage || loaded },
      {
        default: 'flex flex-col items-center rounded-lg ',
        reduced: 'relative center-children'
      }[type]
    ]"
    @click="handleClick"
  >
    <div
      v-if="isImage"
      v-show="loaded || loading"
      class="h-12 w-12 overflow-clip rounded-full shadow center-children"
      :class="{ 'group-hover:opacity-30': loaded }"
    >
      <img
        v-if="src"
        :src="src"
        :alt="alt"
        class="min-h-full min-w-full"
        @load="handleLoad"
      />
      <div
        class="h-6 w-6"
        :class="{
          'group-hover:bg-heroicons-outline-trash-black': loaded
        }"
      ></div>
    </div>
    <p
      v-if="type === 'default'"
      class="break-all text-sm text-slate-700 group-hover:line-through"
    >
      {{ file?.name }}
    </p>
    <p
      class="whitespace-nowrap pt-1 text-xs text-slate-600"
      :class="[
        { 'group-hover:line-through': !isImage || loaded },
        {
          default: '',
          reduced: 'absolute top-full'
        }[type]
      ]"
    >
      {{ fileSize }}
    </p>
  </div>
</template>
