<script setup lang="ts">
import { watch, computed, type PropType, ref } from 'vue'

const emit = defineEmits(['click', 'remove'])

const props = defineProps({
  src: {
    type: Object as PropType<File | string | null>,
    default: null
  },
  alt: {
    type: String,
    default: 'image'
  },
  removable: {
    type: Boolean
  }
})

const loading = ref(false)
const loaded = ref(false)

const src = computed(() => {
  if (props.src instanceof File) {
    if (src.value) {
      URL.revokeObjectURL(src.value)
    }
    return URL.createObjectURL(props.src)
  }

  return props.src
})

watch(
  () => props.src,
  () => {
    loading.value = true
  },
  {
    immediate: true
  }
)

function handleClick() {
  emit('click')
  if (props.removable) {
    emit('remove')
  }
}

function handleLoad() {
  loaded.value = true
  loading.value = false
}
</script>

<template>
  <div
    class="group h-12 w-12 shadow center-children"
    :class="{ 'cursor-pointer hover:opacity-30': removable }"
    @click="handleClick"
  >
    <img
      v-if="src"
      :src="src"
      :alt="alt"
      class="min-h-full min-w-full"
      @load="handleLoad"
    />
    <Mdi:fileImageOutline v-if="!loaded" class="h-6 w-6 text-slate-300" />
    <div
      v-if="removable"
      class="h-6 w-6"
      :class="{
        'group-hover:bg-heroicons-outline-trash-black': loaded
      }"
    ></div>
  </div>
</template>
