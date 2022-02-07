<script setup lang="ts">
import { computed, PropType } from 'vue'

const emit = defineEmits({
  'update:modelValue': (isActive: boolean) => typeof isActive === 'boolean',
  toggle: (isActive: boolean) => typeof isActive === 'boolean'
})

const props = defineProps({
  modelValue: {
    type: Boolean,
    default: true
  },
  duration: {
    type: Number,
    default: 150
  },
  classNames: {
    type: Object as PropType<{
      toggle?: string
      handle?: string
    }>,
    default: {}
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
  <div :class="['toggle', classNames.toggle]" @click="isActive = !isActive">
    <div
      :class="['handle', isActive && 'handle-active', classNames.handle]"
      :style="{
        'transition-duration': `${duration}ms`
      }"
    >
      <slot name="active" v-if="isActive" class="h-6 w-6"></slot>
      <slot name="inactive" v-else></slot>
    </div>
  </div>
</template>

<style lang="postcss" scoped>
.toggle {
  --handle-width: 2rem;
  --toggle-width: calc(2.5 * var(--handle-width));

  @apply relative box-content grid cursor-pointer place-items-center rounded-full border-2 p-1;
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
