<script setup lang="ts">
import { watch, computed, ref, type PropType } from 'vue'

const emit = defineEmits(['click', 'remove'])

const props = defineProps({
  src: {
    type: Object as PropType<File | undefined>,
    default: undefined
  },
  alt: {
    type: String,
    default: 'image'
  }
})

const loading = ref(false)
const loaded = ref(false)

const src = computed(() => {
  if (props.src) {
    if (src.value) {
      URL.revokeObjectURL(src.value)
    }
    return URL.createObjectURL(props.src)
  }

  return undefined
})

const fileSize = computed<string>(() => {
  if (props.src) {
    const sizeInKB = props.src.size / 1024
    return sizeInKB >= 1024
      ? `${(sizeInKB / 1024).toFixed(2)} MB`
      : `${sizeInKB.toFixed(2)} kB`
  }

  return ''
})

watch(
  () => props.src,
  src => {
    if (src) {
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
  emit('click')
  emit('remove')
}

function handleLoad() {
  loaded.value = true
  loading.value = false
}
</script>

<template>
  <div
    class="group relative center-children"
    :class="{ 'cursor-pointer': loaded }"
  >
    <div
      v-show="loaded || loading"
      class="h-12 w-12 overflow-clip rounded-full shadow center-children"
      :class="{ 'group-hover:opacity-30': loaded }"
      @click="handleClick"
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
    <span
      class="absolute top-full whitespace-nowrap pt-1 text-xs text-slate-600"
      :class="{ 'group-hover:line-through': loaded }"
    >
      {{ fileSize }}
    </span>
  </div>
</template>
