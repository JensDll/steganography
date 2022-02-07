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
  <form-file-upload
    label="Image with a hidden message"
    v-model="form.files.$value"
  ></form-file-upload>
</template>

<style lang="postcss" scoped></style>
