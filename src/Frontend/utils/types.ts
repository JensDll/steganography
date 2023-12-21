export type AnyFunction = (...args: any[]) => any

export type NoInfer<A> = [A][A extends any ? 0 : never]

export type FunctionRef = (el: Element | ComponentPublicInstance | null) => void

export interface EventWithTarget<T extends EventTarget> extends Event {
  target: T
}

export interface Lengthy {
  length: number
}

export type AnyRef<T> = Ref<T> | ComputedRef<T>

export type VNodeWithKey = VNode & { key: string }

export interface InputProps<TModelValue> {
  modelValue: TModelValue
  id?: string
}
