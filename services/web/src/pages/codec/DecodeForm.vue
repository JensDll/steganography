<script setup lang="ts">
import { useValidation, type Field } from 'validierung'

type FormData = {
  files: Field<File[]>
}

const { form } = useValidation<FormData>({
  files: {
    $value: []
  }
})
</script>

<template>
  <div class="w-full max-w-5xl">
    <h2 class="mb-8 pl-8">Decode</h2>
    <form
      class="rounded-lg bg-white bg-gradient-to-bl from-emerald-50 shadow-md shadow-slate-200"
    >
      <div class="p-8">
        <FormFileUpload
          v-model="form.files.$value"
          label="Select a cover image"
        ></FormFileUpload>
        <div class="mt-4">
          <label class="label" for="message">Secret key</label>
          <input class="input w-full" type="text" />
        </div>
      </div>
      <div class="flex justify-end bg-emerald-50 px-8 py-4">
        <AppButton class="py-2 px-6" type="decode">Decode</AppButton>
      </div>
    </form>
  </div>
</template>
