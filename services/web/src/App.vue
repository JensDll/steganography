<script setup lang="ts">
const someVar = 'bar'
</script>

<template>
  <TheHeader />
  <TheMain />
  <div :class="`inline sm:block flex${someVar}`"></div>
</template>
