export interface EventWithTarget<T extends EventTarget> extends Event {
  target: T
}

export function useVModelFiles<TEvent extends string>(
  props: { [key in TEvent]: File[] },
  emit: (event: `update:${TEvent}`, value: File[]) => void,
  event: TEvent = 'modelValue' as TEvent,
) {
  let input: HTMLInputElement

  const files = computed<File[]>({
    get() {
      return props[event]
    },
    set(files) {
      emit(`update:${event}`, files)
    },
  })

  const remove = (i: number) => {
    if (input) {
      input.value = ''
    }
    files.value.splice(i, 1)
  }

  const listeners: Record<string, unknown> = {
    change(e: EventWithTarget<HTMLInputElement>) {
      input = e.target

      const fileList = input.files

      if (!fileList) {
        return
      }

      const fileListItems: File[] = []

      for (let i = 0; i < fileList.length; ++i) {
        fileListItems.push(fileList[i])
      }

      files.value = fileListItems
    },
  }

  return {
    files,
    listeners,
    remove,
  }
}
