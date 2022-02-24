<script setup lang="ts">
import { useValidation, type Field } from 'validierung'

import { rules, useApi } from '~/domain'

type FormData = {
  message: Field<string>
  coverImage: Field<File[]>
}

const { form, validateFields } = useValidation<FormData>({
  message: {
    $value: '',
    $rules: [rules.required('Please enter a message')]
  },
  coverImage: {
    $value: [],
    $rules: [rules.minMax(1, 1)('Please select a cover image')]
  }
})

const { loading, api } = useApi()

async function handleSubmit() {
  try {
    const formData = await validateFields()
    await api.encodeText(formData.coverImage[0], formData.message)
  } catch (e) {
    console.log(e)
  }
}
</script>

<template>
  <AppSection>
    <form class="encode" @submit.prevent="handleSubmit">
      <section class="container py-12">
        <div>
          <label class="label" for="message">Secret message</label>
          <textarea
            id="message"
            v-model="form.message.$value"
            placeholder="Better not trust me to much"
            class="max-h-[400px] min-h-[150px] w-full"
            :class="{ error: form.message.$hasError }"
          />
          <FormErrors :errors="form.message.$errors" />
        </div>
        <FormFileInput
          v-model="form.coverImage.$value"
          :errors="form.coverImage.$errors"
          label="Attach a cover image"
          class="mt-6"
        />
      </section>
      <section class="bg-emerald-50 py-4">
        <div class="container flex justify-end">
          <AppButton type="encode" html-type="submit">Encode</AppButton>
        </div>
      </section>
    </form>
  </AppSection>
</template>

<style scoped></style>
