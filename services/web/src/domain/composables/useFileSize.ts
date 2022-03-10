import { computed, isRef, type ComputedRef } from 'vue'

import type { AnyRef } from '..'

export function useFileSize(files: AnyRef<File[]>): ComputedRef<string>

export function useFileSize(
  files: { file?: File },
  key?: 'file'
): ComputedRef<string>

export function useFileSize<T extends { [key in K]?: File }, K extends string>(
  files: T,
  key: K
): ComputedRef<string>

export function useFileSize(
  files: { [key: string]: File | undefined } | AnyRef<File[]>,
  key = 'file'
) {
  return computed<string>(() => {
    const size = isRef(files)
      ? files.value.reduce((sum, file) => sum + file.size, 0)
      : files[key]?.size ?? 0

    if (size) {
      const sizeInKB = size / 1024
      return sizeInKB >= 1024
        ? `${(sizeInKB / 1024).toFixed(2)} MB`
        : `${sizeInKB.toFixed(2)} kB`
    }

    return ''
  })
}
