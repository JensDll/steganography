import type {
  BaseTransitionProps,
  ComponentPublicInstance,
  RendererElement
} from 'vue'

export type AnyFunction = (...args: any[]) => any

export type NoInfer<A> = [A][A extends any ? 0 : never]

export type AnimationHooks<HostElement = RendererElement> = {
  [K in keyof BaseTransitionProps as K extends `on${infer HookName}`
    ? Uncapitalize<HookName>
    : never]: (
    // @ts-expect-error Ignore Parameter's generic constraint
    ...args: Parameters<BaseTransitionProps[K]> extends [any, infer Done]
      ? [el: HostElement, done: Done]
      : [el: HostElement]
  ) => // @ts-expect-error Ignore ReturnType's generic constraint
  ReturnType<BaseTransitionProps[K]>
}

export type FunctionRef = (el: Element | ComponentPublicInstance | null) => void

export interface EventWithTarget<T extends EventTarget> extends Event {
  target: T
}

export interface Lengthy {
  length: number
}
