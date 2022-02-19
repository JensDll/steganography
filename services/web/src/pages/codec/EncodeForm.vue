<script setup lang="ts">
import { useValidation, type Field } from 'validierung'

type FormData = {
  message: Field<string>
  files: Field<File[]>
}

const { form } = useValidation<FormData>({
  message: {
    $value: ''
  },
  files: {
    $value: []
  }
})

function handleSubmit() {
  console.log('submit')
}
</script>

<template>
  <AppSection>
    <form @submit.prevent="handleSubmit">
      <section class="pt-12 container">
        <div>
          <label class="label" for="message">Secret message</label>
          <textarea
            id="message"
            v-model="form.message.$value"
            class="max-h-[400px] min-h-[100px] w-full lg:min-h-[200px]"
          />
        </div>
      </section>
      <section class="mt-8 bg-blue-50 py-4">
        <div class="flex justify-end container">
          <AppButton type="encode" html-type="submit">Encode</AppButton>
        </div>
      </section>
    </form>
  </AppSection>
</template>

<style scoped></style>
