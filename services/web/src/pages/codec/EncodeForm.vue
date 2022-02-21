<script setup lang="ts">
import { useValidation, type Field } from 'validierung'

type FormData = {
  message: Field<string>
  coverImage: Field<File[]>
}

const { form, validateFields } = useValidation<FormData>({
  message: {
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
  <AppSection>
    <form @submit.prevent="handleSubmit">
      <section class="container py-12">
        <div>
          <label class="label" for="message">Secret message</label>
          <textarea
            id="message"
            v-model="form.message.$value"
            placeholder="Better not trust me to much"
            class="max-h-[400px] min-h-[100px] w-full lg:min-h-[200px]"
          />
        </div>
        <FormFileInput v-model="form.coverImage.$value" class="mt-6" />
      </section>
      <section class="bg-blue-50 py-4">
        <div class="container flex justify-end">
          <AppButton type="encode" html-type="submit">Encode</AppButton>
        </div>
      </section>
    </form>
  </AppSection>
</template>

<style scoped></style>
