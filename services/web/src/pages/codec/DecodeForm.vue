<script setup lang="ts">
import { useValidation, type Field } from 'validierung'
import { rules } from '~/domain'

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
    $rules: [rules.minMax(1, 1)('Please select a cover image')]
  }
})

async function handleSubmit() {
  try {
    const formData = await validateFields()
    console.log(formData)
  } catch (e) {
    console.log(e)
  }
}
</script>

<template>
  <AppSection class="justify-self-center">
    <form class="decode" @submit="handleSubmit">
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
          class="mt-6"
        />
      </section>
      <section class="bg-blue-50 py-4">
        <div class="container flex justify-end">
          <AppButton type="decode" html-type="submit">Decode</AppButton>
        </div>
      </section>
    </form>
  </AppSection>
</template>

<style scoped></style>
