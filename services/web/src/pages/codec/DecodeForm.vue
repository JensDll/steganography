<script setup lang="ts">
import { type Field, useValidation } from 'validierung'
import { ref } from 'vue'

import { VErrorListAdd, VErrorListClear } from '~/components/app/VErrorList.vue'
import { api, rules } from '~/domain'
import { ApiError } from '~/domain/api/apiError'

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

const errors = ref<string[]>([])

async function handleSubmit() {
  try {
    const formData = await validateFields()
    await decode(formData.coverImage[0], formData.key)
    VErrorListClear(errors)
  } catch (e) {
    if (e instanceof ApiError) {
      VErrorListAdd(errors, e.message)
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
            class="mr-12 hidden w-full md:grid lg:w-2/3"
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
  <VProgressBar
    class="mx-container mt-8 md:hidden"
    variant="decode"
    :active="loading"
  />
  <VErrorList :errors="errors" />
</template>

<style scoped></style>
