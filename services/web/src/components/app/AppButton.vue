<script setup lang="ts">
import { ref, onMounted, type Ref, type PropType } from 'vue'

import { guards } from '~/domain'

const emit = defineEmits(['click'])

const props = defineProps({
  type: {
    type: String as PropType<'default' | 'encode' | 'decode' | 'landing'>,
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

const buttonRef = ref() as Ref<HTMLButtonElement>
let parentForm: HTMLFormElement | undefined

onMounted(() => {
  if (props.htmlType !== 'submit') {
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
    parentForm.dispatchEvent(
      // eslint-disable-next-line no-undef
      new SubmitEvent('submit', { submitter: buttonRef.value })
    )
  }
}

const eventListeners = {
  click: props.htmlType === 'submit' ? handleSubmit : handleClick
}
</script>

<template>
  <button
    ref="buttonRef"
    :type="htmlType"
    :class="[
      'block rounded border-2 py-1 px-4 font-medium outline-offset-2 focus:outline focus:outline-2',
      {
        default: 'border-gray-300 hover:bg-gray-100 focus:outline-gray-300',
        landing:
          'border-slate-900 bg-slate-900 text-white hover:border-slate-700 hover:bg-slate-700 focus:outline-gray-400',
        encode:
          'border-emerald-500 bg-emerald-500 text-white hover:border-emerald-400 hover:bg-emerald-400 focus:outline-emerald-400',
        decode:
          'border-blue-500 bg-blue-500 text-white hover:border-blue-400 hover:bg-blue-400 focus:outline-blue-400'
      }[type]
    ]"
    :aria-disabled="disabled"
    v-on="eventListeners"
  >
    <slot></slot>
  </button>
</template>
