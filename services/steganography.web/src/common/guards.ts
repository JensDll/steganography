export const isDefinedGuard = <T>(x: T | null | undefined): x is T =>
  x !== null && x !== undefined

export const isFormElementGuard = (el: HTMLElement): el is HTMLFormElement =>
  el.tagName === 'FORM'

export const isImageGuard = (file?: File) => file?.type.startsWith('image/')
