<script setup lang="ts">
import { ref, onMounted, type Ref, type PropType } from 'vue'

import { guards } from '~/domain'

const emit = defineEmits(['click'])

const props = defineProps({
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

const buttonRef = ref() as Ref<HTMLButtonElement>
let parentForm: HTMLFormElement | undefined

onMounted(() => {
  if (props.type !== 'submit') {
    return
  }
  for (
    let el: HTMLElement | null = buttonRef.value;
    el;
    el = el.parentElement
  ) {
    if (guards.isFormElement(el)) {
      parentForm = el
    }
  }
})

const handleClick = (e: MouseEvent) => {
  const target = e.target as HTMLButtonElement
  if (!target.disabled && target.ariaDisabled !== 'true') {
    emit('click')
  }
}

const handleSubmit = (e: MouseEvent) => {
  e.preventDefault()
  const target = e.target as HTMLButtonElement
  if (!target.disabled && target.ariaDisabled !== 'true' && parentForm) {
    parentForm.dispatchEvent(new Event('submit'))
  }
}

const eventListeners = {
  click: props.type === 'submit' ? handleSubmit : handleClick
}
</script>

<template>
  <button
    ref="buttonRef"
    :type="type"
    :class="[
      `block rounded border-2 py-1 px-4 font-medium outline-offset-2 focus:outline focus:outline-2`,
      {
        default: `border-gray-300 hover:bg-gray-100 focus:outline-gray-300`,
        landing: `border-gray-900 bg-gray-900 text-white hover:border-gray-700 hover:bg-gray-700 focus:outline-gray-400
          disabled:!border-gray-900 disabled:!bg-gray-900`,
        encode: `border-encode-500 bg-encode-500 text-white hover:border-encode-400 hover:bg-encode-400 focus:outline-encode-400
           disabled:!border-encode-500 disabled:!bg-encode-500`,
        decode: `border-decode-500 bg-decode-500 text-white hover:border-decode-400 hover:bg-decode-400
          focus:outline-decode-400 disabled:!border-decode-500 disabled:!bg-decode-500`
      }[variant],
      {
        'cursor-not-allowed opacity-30': disabled
      }
    ]"
    :disabled="disabled"
    v-on="eventListeners"
  >
    <slot></slot>
  </button>
</template>
