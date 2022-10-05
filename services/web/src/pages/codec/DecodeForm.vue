<script setup lang="ts">
import { type Field, ValidationError, useValidation } from 'validierung'
import { ref } from 'vue'

import { animation, api, rules } from '~/domain'
import { FetchError } from '~/domain/composables/useFetch'

type FormData = {
  key: Field<string>
  coverImage: Field<File[]>
}

const { form, validateFields } = useValidation<FormData>({
  key: {
    $value: '',
    $rules: [rules.required('Please enter a key')]
  },
  coverImage: {
    $value: [],
    $rules: [rules.minMax(1, 1)('Please attach a cover image')]
  }
})

const { loading, abort, decode } = api.codec()

const errorMessage = ref('')

async function handleSubmit() {
  try {
    const formData = await validateFields()
    await decode(formData.coverImage[0], formData.key)
    errorMessage.value = ''
  } catch (error) {
    if (!(error instanceof ValidationError) && !(error instanceof FetchError)) {
      if (errorMessage.value) {
        animation.shake('#error-message')
      }
      errorMessage.value = 'Decoding failed. Maybe your key is not valid.'
    } else {
      errorMessage.value = ''
    }
  }
}
</script>

<template>
  <FormProvider class="justify-self-center">
    <form @submit.prevent="handleSubmit">
      <section class="container py-12">
        <div>
          <label class="label" for="key">Key phrase</label>
          <input
            id="key"
            v-model="form.key.$value"
            class="w-full"
            :class="{ error: form.key.$hasError }"
            type="password"
          />
          <FormErrors :errors="form.key.$errors" />
        </div>
        <FormFileInput
          v-model="form.coverImage.$value"
          :errors="form.coverImage.$errors"
          label="Attach a cover image"
          accept=".png"
          class="mt-6"
        />
      </section>
      <section class="bg-decode-100 py-4 dark:bg-decode-800">
        <div
          class="container grid grid-cols-[1fr_auto] gap-x-8 md:gap-x-12"
          :class="{ 'justify-between': loading }"
        >
          <VProgressBar
            class="mr-12 w-full lg:w-2/3"
            variant="decode"
            :active="loading"
          />
          <div class="flex grid-area-[1/2/2/3]">
            <VButton class="mr-4" @click="abort.value()">Cancel</VButton>
            <VButton type="submit" variant="decode" :disabled="loading">
              Decode
            </VButton>
          </div>
        </div>
      </section>
    </form>
  </FormProvider>
  <div class="container mt-10">
    <ul>
      <Transition v-on="animation.appear">
        <li
          v-if="errorMessage"
          id="error-message"
          class="flex items-center justify-between bg-red-50 px-6 py-4 text-error dark:bg-red-900/60 dark:text-red-300"
        >
          {{ errorMessage }}
          <div
            class="dark:bg ml-4 cursor-pointer rounded-full bg-red-100 p-1 hover:bg-red-200/60 dark:bg-red-300/25 dark:hover:bg-red-200/30"
            @click="errorMessage = ''"
          >
            <div class="i-heroicons-x-mark"></div>
          </div>
        </li>
      </Transition>
    </ul>
  </div>
</template>

<style scoped></style>
