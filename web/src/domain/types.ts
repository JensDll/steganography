import { BaseTransitionProps, RendererElement } from 'vue'

export type AnyFunction = (...args: any[]) => any

export type AnimationHooks<HostElement = RendererElement> = {
  [K in keyof BaseTransitionProps as K extends `on${infer HookName}`
    ? Uncapitalize<HookName>
    : never]: (
    // @ts-expect-error
    ...args: Parameters<BaseTransitionProps[K]> extends [any, infer Done]
      ? [el: HostElement, done: Done]
      : [el: HostElement]
  ) => // @ts-expect-error
  ReturnType<BaseTransitionProps[K]>
}
