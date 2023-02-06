<script setup lang="ts">
import type { PropType } from 'vue'

const emit = defineEmits(['click'])

defineProps({
  variant: {
    type: String as PropType<'default' | 'encode' | 'decode' | 'landing'>,
    default: 'default'
  },
  type: {
    type: String as PropType<'button' | 'submit' | 'reset'>,
    default: 'button'
  },
  disabled: {
    type: Boolean,
    default: false
  }
})

const handleClick = (e: MouseEvent) => {
  const target = e.target as HTMLButtonElement
  if (!target.disabled && target.ariaDisabled !== 'true') {
    emit('click')
  }
}
</script>

<template>
  <button
    :type="type"
    class="group relative flex items-center rounded px-4 py-2 font-medium outline-offset-2 focus:outline focus:outline-2"
    :class="[
      {
        default:
          'bg-white shadow  hover:bg-gray-100 focus:outline-gray-400  dark:bg-gray-800  dark:hover:bg-gray-700 dark:focus:outline-gray-400',
        landing:
          ' bg-gray-900 px-6 text-white outline-none hover:bg-gray-700 dark:hover:bg-gray-800',
        encode:
          'bg-encode-500 text-white hover:bg-encode-400 focus:outline-encode-300 dark:bg-encode-700 dark:hover:bg-encode-600 dark:focus:outline-encode-600',
        decode:
          'bg-decode-500 text-white hover:bg-decode-400 focus:outline-decode-300 dark:bg-decode-700 dark:hover:bg-decode-600 dark:focus:outline-decode-600'
      }[variant],
      {
        'pointer-events-none opacity-25': disabled
      }
    ]"
    :disabled="disabled"
    @click="handleClick"
  >
    <div
      v-if="variant === 'landing'"
      class="absolute -inset-0.5 -z-10 bg-gradient-to-r from-encode-500 to-decode-500 opacity-75 blur"
    ></div>
    <slot></slot>
  </button>
</template>
