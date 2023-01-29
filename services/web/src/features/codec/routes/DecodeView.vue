<script setup lang="ts">
import { type Field, useValidation } from 'validierung'
import { ref, type VNode } from 'vue'

import { decode } from '../api'
import { required, minMax } from '~/common/rules'
import {
  BaseErrorListAdd,
  BaseErrorListClear
} from '~/components/base/BaseErrorList.vue'
import { ApiError } from '~/composables'

type FormData = {
  key: Field<string>
  coverImage: Field<File[]>
}

const { form, validateFields } = useValidation<FormData>({
  key: {
    $value: '',
    $rules: [required('Please enter a key')]
  },
  coverImage: {
    $value: [],
    $rules: [minMax(1, 1)('Please attach a cover image')]
  }
})

const errors = ref<VNode[]>([])

async function handleSubmit() {
  try {
    const formData = await validateFields()
    await decode(formData.coverImage[0], formData.key)
    BaseErrorListClear(errors)
  } catch (error) {
    if (error instanceof ApiError) {
      BaseErrorListAdd(errors, error.vNode)
    }
  }
}
</script>

<template>
  <form @submit.prevent="handleSubmit">
    <section>
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
        accept=".png"
        class="mt-6"
      />
    </section>
    <section class="mt-10 flex items-center justify-end">
      <LoadingIndicator
        class="mr-6 text-highlight-decode"
        :loading="decode.loading.value"
      />
      <BaseButton class="mr-4" @click="decode.abort()">Cancel</BaseButton>
      <BaseButton
        type="submit"
        variant="decode"
        :disabled="decode.loading.value"
      >
        <span class="i-mdi-image-search-outline mr-1 inline-block"></span>
        Decode
      </BaseButton>
    </section>
  </form>
  <BaseErrorList :errors="errors" />
</template>

<style scoped></style>
