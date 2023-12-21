<script setup lang="ts">
import { type Field, useValidation } from 'validierung'
import { ref, computed } from 'vue'

import FileInput from '~/components/FileInput.vue'

type FormData = {
  textData: Field<string>
  binaryData: Field<File[]>
  coverImage: Field<File[]>
}

const messageMode = ref<'text' | 'binary'>('text')
const isTextMode = computed(() => messageMode.value === 'text')
const isBinaryMode = computed(() => messageMode.value === 'binary')

const { form, validateFields, errors } = useValidation<FormData>({
  textData: {
    $value: '',
    $rules: [withPrecondition(isTextMode)(required('Please enter a message'))],
  },
  binaryData: {
    $value: [],
    $rules: [
      withPrecondition(isBinaryMode)(min(1)('Please attach one or more files')),
    ],
  },
  coverImage: {
    $value: [],
    $rules: [minMax(1, 1)('Please attach a cover image')],
  },
})

async function handleSubmit() {
  try {
    const formData = await validateFields()
    console.log(formData)
  } catch (error) {
    console.log(errors.value)
  }
}
</script>

<template>
  <section class="pb-12 pt-8">
    <h1 class="mb-6 text-3xl font-bold container">Encode</h1>
    <form @submit.prevent="handleSubmit">
      <div class="container">
        <section>
          <div class="mb-4">
            <label for="message" class="mb-0">Secret message</label>
            <fieldset class="mb-2 flex">
              <label class="mb-0 flex items-center font-normal">
                <input
                  v-model="messageMode"
                  class="mr-2 cursor-pointer"
                  type="radio"
                  name="messageType"
                  value="text"
                />
                Text data
              </label>
              <label class="mb-0 ml-4 flex items-center font-normal">
                <input
                  v-model="messageMode"
                  class="mr-2 cursor-pointer"
                  type="radio"
                  name="messageType"
                  value="binary"
                />
                Binary data
              </label>
            </fieldset>
            <textarea
              v-if="isTextMode"
              id="message"
              v-model="form.textData.$value"
              placeholder="Your data will be hidden and encrypted. Nothing gets logged or saved anywhere"
              class="max-h-[24rem] min-h-[8rem] w-full"
              :class="{ 'form-error': form.textData.$hasError }"
            ></textarea>
            <FileInputMultiple
              v-else
              id="message"
              v-model="form.binaryData.$value"
              class="max-h-[24rem] min-h-[8rem]"
            >
              <p class="not-prose">
                Choose files
                <span class="hidden md:inline-block">or drag and drop</span>
                here
              </p>
              <p class="not-prose text-sm">
                Must not be much larger than the cover image
              </p>
            </FileInputMultiple>
          </div>
          <div>
            <label for="cover-image">Cover image</label>
            <FileInput
              id="cover-image"
              v-model="form.coverImage.$value"
              accept="image/*"
            >
            </FileInput>
          </div>
        </section>
        <section class="float-right mt-20">
          <button
            type="submit"
            class="bg-indigo-600 px-4 py-1 font-medium text-white hover:bg-indigo-500"
          >
            Encode
          </button>
        </section>
      </div>
    </form>
  </section>
</template>

<style scoped></style>
