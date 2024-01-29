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

const pending = computed(
  () => encodeText.pending.value || encodeBinary.pending.value,
)

const abort = () => {
  encodeText.abort()
  encodeBinary.abort()
}

async function handleSubmit() {
  try {
    const formData = await validateFields()
    if (messageMode.value === 'text') {
      await encodeText(formData.coverImage[0], formData.textData)
    } else {
      await encodeBinary(formData.coverImage[0], formData.binaryData)
    }
  } catch (error) {}
}
</script>

<template>
  <section class="pb-24 pt-8">
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
              class="max-h-96 min-h-32 w-full"
              :class="{ 'form-error': form.textData.$hasError }"
            ></textarea>
            <AppFileInputMultiple
              v-else
              id="message"
              v-model="form.binaryData.$value"
              class="min-h-32"
            >
              <p>
                <span class="font-semibold text-green-600">Choose files </span>
                <span class="hidden md:inline-block">or drag and drop</span>
                here
              </p>
            </AppFileInputMultiple>
          </div>
          <div>
            <label for="cover-image">Cover image</label>
            <AppFileInput
              id="cover-image"
              accept="image/*"
              v-model="form.coverImage.$value"
            >
              <span class="font-semibold text-green-600">Choose </span>
              <span class="hidden md:inline-block">or drag and drop</span> here
            </AppFileInput>
          </div>
        </section>
        <section class="mt-12 flex items-center justify-end space-x-4">
          <div class="flex items-center space-x-4 pr-4" v-if="pending">
            <span class="text-sm">please wait</span>
            <div
              class="i-svg-spinners-bars-rotate-fade ml-2 text-green-600"
            ></div>
          </div>
          <button
            type="button"
            class="rounded border bg-gray-100 px-3 py-2 font-medium hover:bg-gray-50"
            @click="abort"
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
    <section class="mt-6 container">
      <ul>
        <li v-for="error in errors" :key="error">{{ error }}</li>
      </ul>
    </section>
  </section>
</template>
