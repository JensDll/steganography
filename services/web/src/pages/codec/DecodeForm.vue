<script setup lang="ts">
import { useValidation, type Field } from 'validierung'
import { rules, useApi } from '~/domain'

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

const { loading, codec } = useApi()

async function handleSubmit() {
  try {
    const formData = await validateFields()
    await codec.decode(formData.coverImage[0], formData.key)
  } catch (e) {
    console.log(e)
  }
}
</script>

<template>
  <AppSection class="justify-self-center">
    <form class="decode" @submit="handleSubmit">
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
          class="mt-6"
        />
      </section>
      <section class="bg-decode-50 py-4">
        <div class="flex justify-end container">
          <AppButton type="submit" variant="decode"> Decode</AppButton>
        </div>
      </section>
    </form>
  </AppSection>
</template>

<style scoped></style>
