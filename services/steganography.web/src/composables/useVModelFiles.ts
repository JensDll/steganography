import { type ComputedRef, computed } from 'vue'

import type { EventWithTarget } from '~/common/types'

export type FileHelper = {
  file: File
  size: string
}

export function useVModelFiles<
  TProps extends Record<'modelValue', any>,
  TEmit extends (event: `update:modelValue`, value: TProps['modelValue']) => any
>(
  props: TProps,
  emit: TEmit
): {
  files: ComputedRef<File[]>
  removeFile: (i: number) => void
  fileListeners: Record<string, (e: Event) => void>
}

export function useVModelFiles<
  TProps extends Record<TEvent, any>,
  TEmit extends (event: `update:${TEvent}`, value: TProps[TEvent]) => any,
  TEvent extends string
>(
  props: TProps,
  emit: TEmit,
  event: TEvent
): {
  files: ComputedRef<File[]>
  removeFile: (i: number) => void
  fileListeners: Record<string, (e: Event) => void>
}

export function useVModelFiles(
  props: Record<string, any>,
  emit: (event: `update:${string}`, value: unknown) => any,
  event = 'modelValue'
) {
  let input: HTMLInputElement

  const files = computed<File[]>({
    get() {
      return props[event]
    },
    set(files) {
      emit(`update:${event}`, files)
    }
  })

  const removeFile = (i: number) => {
    if (input) {
      input.value = ''
    }
    files.value.splice(i, 1)
  }

  const change = (e: EventWithTarget<HTMLInputElement>) => {
    input = e.target
    const fileList = input.files

    if (fileList) {
      const fileListItems: File[] = []

      for (let i = 0; i < fileList.length; i++) {
        fileListItems.push(fileList[i])
      }

      files.value = fileListItems
    }
  }

  const fileListeners: Record<string, any> = {
    change
  }

  return {
    files,
    removeFile,
    fileListeners
  }
}
