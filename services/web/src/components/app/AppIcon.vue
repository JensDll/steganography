<script setup lang="ts">
import { type PropType } from 'vue'

type IconName = 'ImagePlus'

const icons: Record<`./icon/${IconName}.vue`, any> = import.meta.globEager(
  './icons/*.vue'
)

defineProps({
  icon: {
    type: String as PropType<IconName>,
    required: true
  }
})
</script>

<template>
  <component :is="icons[`./icon/${icon}.vue`].default" />
</template>
