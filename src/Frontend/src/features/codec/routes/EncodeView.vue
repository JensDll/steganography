<script setup lang="ts">
import { ref, computed } from 'vue'

const messageMode = ref<'text' | 'binary'>('text')
const isTextMode = computed(() => messageMode.value === 'text')
// const isBinaryMode = computed(() => messageMode.value === 'binary')
</script>

<template>
  <h2 class="container">Encode</h2>
  <form
    class="bg-gray-50 pb-10 pt-6 xl:mx-auto xl:w-[calc(var(--max-w-container)+4rem)] xl:rounded-xl"
  >
    <div class="container">
      <section>
        <div class="mb-4">
          <label for="message" class="mb-0">Secret message</label>
          <div class="mb-2 flex">
            <label class="mb-0 flex cursor-pointer items-center font-normal">
              <input
                v-model="messageMode"
                class="mr-1 cursor-pointer"
                type="radio"
                name="messageType"
                value="text"
              />
              Text data
            </label>
            <label
              class="mb-0 ml-4 flex cursor-pointer items-center font-normal"
            >
              <input
                v-model="messageMode"
                class="mr-1 cursor-pointer"
                type="radio"
                name="messageType"
                value="binary"
              />
              Binary data
            </label>
          </div>
          <textarea
            v-if="isTextMode"
            id="message"
            placeholder="Your data will be hidden and encrypted. Nothing gets logged or saved anywhere"
            class="max-h-[24rem] min-h-[8rem] w-full"
          />
          <input v-else id="message" type="file" multiple />
        </div>
        <div>
          <label for="cover-image">Cover image</label>
          <input id="cover-image" type="file" accept="image/*" />
        </div>
      </section>
      <section class="float-right mt-20">
        <button
          type="submit"
          class="bg-black px-4 py-1 font-medium text-white hover:bg-gray-700"
        >
          Encode
        </button>
      </section>
    </div>
  </form>
</template>

<style scoped></style>
