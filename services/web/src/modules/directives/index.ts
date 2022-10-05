import type { Plugin } from 'vue'

import { onClickOutside } from '~/modules/directives/onClickOutside'

export const directives: Plugin = app => {
  app.directive('onClickOutside', onClickOutside)
}
