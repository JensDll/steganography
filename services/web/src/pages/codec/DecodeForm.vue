<script setup lang="ts">
import { useValidation, type Field } from 'validierung'

type FormData = {
  key: Field<string>
  coverImage: Field<File[]>
}

const { form, validateFields } = useValidation<FormData>({
  key: {
    $value: ''
  },
  coverImage: {
    $value: []
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
            type="password"
          />
        </div>
        <FormFileInput v-model="form.coverImage.$value" class="mt-6" />
      </section>
      <section class="bg-pink-50 py-4">
        <div class="container flex justify-end">
          <AppButton type="decode" html-type="submit">Decode</AppButton>
        </div>
      </section>
    </form>
  </AppSection>
</template>

<style scoped></style>
