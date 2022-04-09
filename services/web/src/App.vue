<script setup lang="ts">
import { ref } from 'vue'
import { Discovery } from './domain/api/serviceDiscovery'

const uri = ref('')

async function handleDiscover() {
  const instances = await Discovery.discoverInstances('api')
  console.log(instances)
}

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
    <AppButton class="mt-4" variant="encode" @click="handleDiscover">
      Discover
    </AppButton>
  </div>
</template>
