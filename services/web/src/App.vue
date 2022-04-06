<script setup lang="ts">
import { ref } from 'vue'

const uri = ref('')

async function handleFetch() {
  const result = await fetch(uri.value)
  console.log(result)
  const txt = await result.text()
  console.log(txt)
}
</script>

<template>
  <TheHeader />
  <TheMain />
  <div class="p-16">
    <input v-model="uri" class="block w-1/2 border p-4" />
    <AppButton class="mt-4" variant="encode" @click="handleFetch">
      Fetch
    </AppButton>
  </div>
</template>
