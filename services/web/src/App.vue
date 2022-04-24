<script setup lang="ts">
import { ref } from 'vue'

const uri = ref('')

async function handleFetch() {
  const response = await fetch(uri.value)
  console.log(response)
  const text = await response.text()
  console.log(text)
}
</script>

<template>
  <TheHeader />
  <TheMain />
  <div>
    <input type="text" v-model="uri" />
    <AppButton @click="handleFetch">Fetch</AppButton>
  </div>
</template>
