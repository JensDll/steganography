<script setup lang="ts">
import type { PropType } from 'vue'

export type SolidIconName = 'LoadingCircle'

export type OutlineIconName = 'Moon' | 'Sun' | 'DesktopComputer'

const icons: Record<string, any> = import.meta.globEager('./icons/**/*.vue')

defineProps({
  solid: {
    type: String as PropType<SolidIconName>,
    default: ''
  },
  outline: {
    type: String as PropType<OutlineIconName>,
    default: ''
  }
})
</script>

<template>
  <component
    :is="
      icons[
        solid
          ? `./icons/solid/${solid}Icon.vue`
          : `./icons/outline/${outline}Icon.vue`
      ].default
    "
  />
</template>
