import type { Plugin } from 'vue'

import { onClickOutside } from './onClickOutside'

export const directives: Plugin = app => {
  app.directive('onClickOutside', onClickOutside)
}
