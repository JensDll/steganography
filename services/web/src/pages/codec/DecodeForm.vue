<script setup lang="ts">
import { useValidation, type Field } from 'validierung'

type FormData = {
  key: Field<string>
}

const { form } = useValidation<FormData>({
  key: {
    $value: ''
  }
})

function handleSubmit() {
  console.log('submit')
}
</script>

<template>
  <AppSection class="justify-self-center">
    <form class="decode" @submit="handleSubmit">
      <section class="pt-12 container">
        <div>
          <label class="label" for="key">Key phrase</label>
          <input
            id="key"
            v-model="form.key.$value"
            class="w-full"
            type="text"
          />
        </div>
      </section>
      <section class="mt-12 bg-pink-50 py-4">
        <div class="flex justify-end container">
          <AppButton type="decode" html-type="submit">Decode</AppButton>
        </div>
      </section>
    </form>
  </AppSection>
</template>

<style scoped></style>
