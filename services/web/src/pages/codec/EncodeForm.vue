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
    <h1 class="mb-12 text-center">Encode</h1>
    <form>
      <div class="bg-gradient-to-br from-sky-50 p-6">
        <div>
          <label class="label" for="message">Secret message</label>
          <textarea
            id="message"
            class="input max-h-[400px] min-h-[100px] w-full lg:min-h-[200px]"
          />
        </div>
        <FormFileUpload
          v-model="form.files.$value"
          class="mt-4"
          label="Select a cover image"
        ></FormFileUpload>
      </div>
      <div class="flex justify-end bg-sky-100 p-6">
        <AppButton class="py-2 px-6" type="encode">Encode</AppButton>
      </div>
    </form>
  </div>
</template>

<style>
.my-shadow {
  box-shadow: 0 0 8px 2px rgb(203 213 225 / 0.3);
}
</style>
