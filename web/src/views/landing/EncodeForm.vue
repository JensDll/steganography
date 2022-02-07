<script setup lang="ts">
import { Field, useValidation } from 'validierung'

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
  <div>
    <h2 class="mb-6 text-center">Encode</h2>
    <form class="pb-4">
      <div>
        <label class="label" for="message">Secret message</label>
        <textarea
          class="input w-full min-h-[100px] max-h-[400px] lg:min-h-[200px]"
          id="message"
        />
      </div>
      <form-file-upload
        class="mt-4"
        label="Select a cover image"
        v-model="form.files.$value"
      ></form-file-upload>
    </form>
  </div>
</template>

<style lang="postcss" scoped></style>
