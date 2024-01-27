<script setup lang="ts">
import { type Field, useValidation } from 'validierung'

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
            <fieldset class="mb-2 mt-1 flex">
              <label class="mb-0 flex items-center font-normal">
                <input
                  v-model="messageMode"
                  class="mr-1 cursor-pointer"
                  type="radio"
                  name="messageType"
                  value="text"
                />
                Text data
              </label>
              <label class="mb-0 ml-4 flex items-center font-normal">
                <input
                  v-model="messageMode"
                  class="mr-1 cursor-pointer"
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
              <p>
                <span class="font-semibold text-green-600">Choose files </span>
                <span class="hidden md:inline-block">or drag and drop</span>
                here
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
              <span class="font-semibold text-green-600">Choose </span>
              <span class="hidden md:inline-block">or drag and drop</span> here
              <template #file-placeholder>No image chosen</template>
            </FileInput>
          </div>
        </section>
        <section class="float-right mt-16 space-x-4">
          <button
            class="rounded border bg-gray-100 px-3 py-2 font-medium hover:bg-gray-50"
          >
            Cancel
          </button>
          <button
            type="submit"
            class="rounded bg-green-600 px-3 py-2 font-semibold text-white hover:bg-green-500"
          >
            Encode
          </button>
        </section>
      </div>
    </form>
  </section>
</template>

<style scoped></style>
