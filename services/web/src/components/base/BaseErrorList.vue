<script lang="ts">
export function BaseErrorListAdd(errors: Ref<VNode[]>, error: VNode) {
  const idx = errors.value.findIndex(e => e.key === error.key)

  if (idx >= 0) {
    animateShake(`#v-error-list-${idx}`)
  } else {
    errors.value.push(error)
  }
}

export function BaseErrorListClear(errors: Ref<VNode[]>) {
  errors.value = []
}
</script>

<script setup lang="ts">
import type { PropType, Ref, VNode } from 'vue'

import { animateShake } from '~/common/animate'

defineProps({
  errors: {
    type: Array as PropType<VNode[]>,
    default: () => []
  }
})

function BaseErrorListRemove(errors: VNode[], idx: number) {
  errors.splice(idx, 1)
}
</script>

<template>
  <Teleport to="body">
    <div class="fixed top-24 z-20 w-full max-w-prose px-container">
      <TransitionGroup name="list" tag="ul" class="relative">
        <li
          v-for="(error, idx) in errors"
          :id="`v-error-list-${idx}`"
          :key="error.key as never"
          class="mb-4 flex w-fit items-center justify-between rounded bg-red-50 bg-opacity-95 px-3 py-2 text-red-900 dark:bg-red-900/80 dark:text-red-300"
        >
          <div
            class="close-icon i-heroicons-x-mark-20-solid order-2 ml-4 shrink-0 cursor-pointer"
            @click="BaseErrorListRemove(errors, idx)"
          ></div>
          <component
            :is="error"
            class="app-markdown text-sm hover:text-opacity-50 prose-a:text-inherit"
          />
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

<style>
.close-icon:hover {
  @apply opacity-50 dark:opacity-70;
}

.close-icon:hover + .app-markdown {
  @apply line-through opacity-50 dark:opacity-70;
}
</style>
