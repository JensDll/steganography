<script setup lang="ts">
import { computed, type PropType } from 'vue'

const emit = defineEmits({
  'update:modelValue': (isActive: boolean) => typeof isActive === 'boolean',
  toggle: (isActive: boolean) => typeof isActive === 'boolean'
})

const props = defineProps({
  modelValue: {
    type: Boolean,
    default: false
  },
  duration: {
    type: Number,
    default: 150
  },
  type: {
    type: String as PropType<'encode' | 'decode'>,
    default: 'encode'
  }
})

const isActive = computed<boolean>({
  get() {
    return props.modelValue
  },
  set(isActive) {
    emit('update:modelValue', isActive)
    emit('toggle', isActive)
  }
})
</script>

<template>
  <div
    :class="[
      'toggle',
      {
        encode: 'bg-emerald-50',
        decode: 'bg-blue-50'
      }[type]
    ]"
    @click="isActive = !isActive"
  >
    <div
      :class="[
        'handle',
        isActive && 'handle-active',
        {
          encode: 'bg-emerald-500',
          decode: 'bg-blue-500'
        }[type]
      ]"
      :style="{
        'transition-duration': `${duration}ms`
      }"
    >
      <slot v-if="isActive" name="active" class="h-6 w-6"></slot>
      <slot v-else name="inactive"></slot>
    </div>
  </div>
</template>

<style scoped>
.toggle {
  --handle-width: 1.25rem;
  --toggle-width: calc(2.5 * var(--handle-width));

  @apply relative box-content grid cursor-pointer place-items-center rounded-full border border-slate-300 p-1 shadow-sm;
  grid-template-columns: var(--handle-width) 1fr var(--handle-width);
  grid-template-areas: 'active . inactive';
  width: var(--toggle-width);
}

:slotted(.handle > *) {
  width: calc(var(--handle-width) / 2);
  height: calc(var(--handle-width) / 2);
}

.handle {
  @apply z-10 grid place-items-center rounded-full;
  width: var(--handle-width);
  height: var(--handle-width);
  grid-area: active;
  transition-property: transform;
}

.handle-active {
  transform: translateX(calc(var(--toggle-width) - var(--handle-width)));
}

.active-slot {
  grid-area: active;
}

.inactive-slot {
  grid-area: inactive;
}
</style>
