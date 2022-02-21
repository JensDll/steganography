import { computed, reactive, type ComponentPublicInstance } from 'vue'

export type FileHelper = {
  file: File
  loading: boolean
  loaded: boolean
}

export function useVModelFiles<
  TProps extends Record<TEvent, File[]>,
  TEmit extends (event: `update:${TEvent}`, value: File[]) => any,
  TEvent extends string = 'modelValue'
>(
  props: TProps,
  emit: TEmit,
  // @ts-expect-error Ignore subtype of constraint warning
  event: TEvent = 'modelValue'
) {
  let fileInput: HTMLInputElement | null
  const fileInputRef = (el: Element | ComponentPublicInstance | null) => {
    fileInput = el as never
  }

  const files = computed<File[]>({
    get() {
      return props[event]
    },
    set(files) {
      emit(`update:${event}`, files)
    }
  })
  const fileHelpers = computed<FileHelper[]>(() =>
    files.value.map(file => reactive({ file, loading: false, loaded: false }))
  )

  const removeFile = (i: number) => {
    if (fileInput) {
      fileInput.value = ''
    }
    files.value.splice(i, 1)
  }

  const handleFileChange = (e: Event) => {
    const input = e.target as HTMLInputElement
    const fileList = input.files

    if (fileList) {
      const fileListItems: File[] = []

      for (let i = 0; i < fileList.length; i++) {
        fileListItems.push(fileList[i])
      }

      files.value = fileListItems
    }
  }

  return {
    files: fileHelpers,
    fileInputRef,
    removeFile,
    handleFileChange
  }
}
