export interface Lengthy {
  length: number
}

export type AnyFunction = (...args: any[]) => any

export const required = (msg: string) => (value: unknown) => !value && msg

export const min = (min: number) => (msg: string) => (value: Lengthy) =>
  value.length >= min || msg

export const max = (max: number) => (msg: string) => (value: Lengthy) =>
  value.length <= max || msg

export const minMax =
  (min: number, max: number) => (msg: string) => (value: Lengthy) =>
    (min <= value.length && value.length <= max) || msg

export const email = (msg: string) => (value: string) =>
  /\S+@\S+\.\S+/.test(value) || msg

export const equal =
  (msg: string) =>
  (...values: unknown[]) =>
    values.every(value => value === values[0]) || msg

export const withPrecondition =
  (...preconditions: (Ref<boolean> | ComputedRef<boolean>)[]) =>
  (rule: AnyFunction) =>
  (...args: unknown[]) =>
    preconditions.every(r => r.value) && rule(...args)
