import { type WritableComputedRef, computed } from 'vue'

export function useVModel<
  TProps extends Record<'modelValue', any>,
  TEmit extends (
    event: `update:modelValue`,
    value: TProps['modelValue'],
  ) => any,
>(
  props: TProps,
  emit: TEmit,
): {
  modelValue: WritableComputedRef<TProps['modelValue']>
}

export function useVModel<
  TProps extends Record<TEvent, any>,
  TEmit extends (event: `update:${TEvent}`, value: TProps[TEvent]) => any,
  TEvent extends string,
>(
  props: TProps,
  emit: TEmit,
  event: TEvent,
): {
  [K in TEvent]: WritableComputedRef<TProps[TEvent]>
}

export function useVModel(
  props: Record<string, any>,
  emit: (event: `update:${string}`, value: unknown) => any,
  event = 'modelValue',
) {
  const data = computed({
    get() {
      return props[event]
    },
    set(value) {
      emit(`update:${event}`, value)
    },
  })

  return {
    [event]: data,
  }
}
