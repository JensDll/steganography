<script lang="ts">
export function VErrorListAdd(errors: Ref<string[]>, error: string) {
  const idx = errors.value.findIndex(e => e === error)

  if (idx >= 0) {
    animation.shake(`#v-error-list-${idx}`)
  } else {
    errors.value.push(error)
  }
}

export function VErrorListClear(errors: Ref<string[]>) {
  errors.value = []
}

function VErrorListRemove(errors: string[], idx: number) {
  errors.splice(idx, 1)
}
</script>

<script setup lang="ts">
import type { PropType, Ref } from 'vue'

import { animation } from '~/domain'

defineProps({
  errors: {
    type: Array as PropType<string[]>,
    default: () => []
  }
})
</script>

<template>
  <Teleport to="body">
    <div
      class="pointer-events-none fixed top-24 z-20 w-full max-w-prose px-container"
    >
      <TransitionGroup name="list" tag="ul" class="relative">
        <li
          v-for="(error, idx) in errors"
          :id="`v-error-list-${idx}`"
          :key="error"
          class="pointer-events-auto mb-4 flex w-fit cursor-pointer select-none items-center justify-between rounded bg-red-50 bg-opacity-90 px-3 py-2 text-red-900 hover:text-opacity-50 hover:line-through dark:bg-red-900/75 dark:text-red-300 dark:hover:text-opacity-70"
          @click="VErrorListRemove(errors, idx)"
        >
          <span class="text-sm">{{ error }}</span>
          <div class="i-heroicons-x-mark-20-solid ml-4 shrink-0"></div>
        </li>
      </TransitionGroup>
    </div>
  </Teleport>
</template>

<style scoped>
.list-move,
.list-enter-active,
.list-leave-active {
  transition-property: transform, opacity;
  transition-timing-function: ease;
  transition-duration: 300ms;
}

.list-enter-from,
.list-leave-to {
  opacity: 0;
  transform: translate(-1.5rem, 0);
}

.list-leave-active {
  position: absolute;
}
</style>
