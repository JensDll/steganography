import type { ComputedRef, Ref } from 'vue'

import type { AnyFunction, Lengthy } from './types'

export const rules = {
  required: (msg: string) => (value: unknown) => !value && msg,
  min: (min: number) => (msg: string) => (value: Lengthy) =>
    value.length >= min || msg,
  max: (max: number) => (msg: string) => (value: Lengthy) =>
    value.length <= max || msg,
  minMax: (min: number, max: number) => (msg: string) => (value: Lengthy) =>
    (min <= value.length && value.length <= max) || msg,
  email: (msg: string) => (value: string) => /\S+@\S+\.\S+/.test(value) || msg,
  equal:
    (msg: string) =>
    (...values: unknown[]) =>
      values.every(value => value === values[0]) || msg,
  withPrecondition:
    (...pre: (Ref<boolean> | ComputedRef<boolean>)[]) =>
    (rule: AnyFunction) =>
    (...args: unknown[]) =>
      pre.every(r => r.value) && rule(...args)
}
