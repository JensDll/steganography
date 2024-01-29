import { createValidation } from 'validierung'

export default defineNuxtPlugin(nuxtApp => {
  const validierung = createValidation({
    defaultValidationBehavior: 'lazier',
    validationBehavior: {
      change: ({ force }) => !force,
      lazy: ({ touched }) => touched,
      lazier: ({ force, touched, submit, hasError }) =>
        force || submit || (touched && hasError),
      submit: ({ submit, hasError }) => submit || hasError,
    },
  })

  nuxtApp.vueApp.use(validierung)
})

declare module 'validierung' {
  interface ValidationBehaviorFunctions {
    change: never
    lazy: never
    lazier: never
    submit: never
  }
}
