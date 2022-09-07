<script setup lang="ts">
import { type Field, ValidationError, useValidation } from 'validierung'
import { ref } from 'vue'

import { animation, api, rules } from '~/domain'

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

const { loading, decode } = api.codec()
const errorMessage = ref('')

async function handleSubmit() {
  try {
    const formData = await validateFields()
    await decode(formData.coverImage[0], formData.key)
    errorMessage.value = ''
  } catch (error) {
    if (import.meta.env.DEV) {
      console.log(error)
    }

    if (!(error instanceof ValidationError)) {
      if (errorMessage.value) {
        animation.shake('#error-message')
      }
      errorMessage.value = 'Decoding failed. Maybe your key is not valid'
    } else {
      errorMessage.value = ''
    }
  }
}
</script>

<template>
  <AppSection class="justify-self-center">
    <form @submit.prevent="handleSubmit">
      <section class="py-12 container">
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
      <section class="bg-decode-100 py-4 dark:bg-decode-900">
        <div
          class="grid grid-cols-[1fr_auto] gap-x-8 container md:gap-x-12"
          :class="{ 'justify-between': loading }"
        >
          <AppProgressBar
            class="mr-12 w-full lg:w-2/3"
            variant="decode"
            :active="loading"
          />

          <AppButton
            type="submit"
            variant="decode"
            class="grid-area-[1/2/2/3]"
            :disabled="loading"
          >
            Decode
          </AppButton>
        </div>
      </section>
    </form>
  </AppSection>
  <div class="mt-10 container">
    <Transition v-on="animation.appear">
      <p v-if="errorMessage" id="error-message" class="text-text-error">
        {{ errorMessage }}
      </p>
    </Transition>
  </div>
</template>

<style scoped></style>
