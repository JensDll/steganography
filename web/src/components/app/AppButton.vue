<script setup lang="ts">
import { ref, onMounted, Ref, PropType } from 'vue'

import { guards } from '~/domain'

const emit = defineEmits(['click'])

const props = defineProps({
  type: {
    type: String as PropType<'default' | 'primary' | 'danger' | 'landing'>,
    default: 'default'
  },
  htmlType: {
    type: String as PropType<'button' | 'submit' | 'reset'>,
    default: 'button'
  },
  disabled: {
    type: Boolean,
    default: false
  }
})

const button = ref() as Ref<HTMLButtonElement>
let form: HTMLFormElement | null = null

onMounted(() => {
  if (props.htmlType !== 'submit') {
    return
  }
  for (let el: HTMLElement | null = button.value; el; el = el.parentElement) {
    if (guards.isFormElement(el)) {
      form = el
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
  if (!target.disabled && target.ariaDisabled !== 'true' && form) {
    form.dispatchEvent(new SubmitEvent('submit', { submitter: button.value }))
  }
}

const eventListeners = {
  click: props.htmlType === 'submit' ? handleSubmit : handleClick
}
</script>

<template>
  <button
    :type="htmlType"
    :class="[
      'block rounded border-2 py-1 px-4 font-medium outline-offset-2 transition-colors focus:outline',
      {
        default: 'border-gray-300 hover:bg-gray-100 focus:outline-gray-300',
        landing:
          'border-slate-900 bg-slate-900 text-white hover:border-slate-700 hover:bg-slate-700 focus:outline-gray-400',
        primary:
          'border-sky-500 bg-sky-500 text-white hover:border-sky-600 hover:bg-sky-600 focus:outline-sky-500',
        danger:
          'border-red-500 bg-red-500 text-white hover:border-red-600 hover:bg-red-600 focus:outline-red-500'
      }[type]
    ]"
    :aria-disabled="disabled"
    ref="button"
    v-on="eventListeners"
  >
    <slot></slot>
  </button>
</template>
