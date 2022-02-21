import { computed, type WritableComputedRef } from 'vue'

import { type NoInfer } from '..'

export function useVModel<
  TProps extends Record<TEvent, any>,
  TEmit extends (event: `update:${TEvent}`, value: TProps[TEvent]) => any,
  TEvent extends string = 'modelValue'
>(
  props: TProps,
  emit: TEmit,
  // @ts-expect-error Ignore subtype of constraint warning
  event: TEvent = 'modelValue'
): {
  [K in NoInfer<TEvent>]: WritableComputedRef<TProps[K]>
} {
  const data = computed<TProps[TEvent]>({
    get() {
      return props[event]
    },
    set(value) {
      emit(`update:${event}`, value)
    }
  })

  return {
    [event]: data
  } as never
}
