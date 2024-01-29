<script setup lang="ts">
interface Props {
  file: File
}

const props = defineProps<Props>()

const src = computed(() => URL.createObjectURL(props.file))
</script>

<template>
  <img :src="src" />
</template>

<style scoped></style>
