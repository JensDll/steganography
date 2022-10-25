<script setup lang="ts">
import { type Field, useValidation } from 'validierung'
import { ref, type VNode } from 'vue'

import { decode } from '../api'
import { required, minMax } from '~/common/rules'
import {
  BaseErrorListAdd,
  BaseErrorListClear
} from '~/components/base/BaseErrorList.vue'
import { ApiError } from '~/composables/useFetch'

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
  <FormProvider class="justify-self-center">
    <form @submit.prevent="handleSubmit">
      <section class="container py-12">
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
      <section class="bg-decode-100 py-4 dark:bg-decode-800">
        <div
          class="container grid grid-cols-[1fr_auto] gap-x-8 md:gap-x-12"
          :class="{ 'justify-between': decode.loading.value }"
        >
          <BaseProgressBar
            class="mr-12 hidden w-full md:grid lg:w-2/3"
            variant="decode"
            :active="decode.loading.value"
          />
          <div class="flex grid-area-[1/2/2/3]">
            <BaseButton class="mr-4" @click="decode.abort()">Cancel</BaseButton>
            <BaseButton
              type="submit"
              variant="decode"
              :disabled="decode.loading.value"
            >
              Decode
            </BaseButton>
          </div>
        </div>
      </section>
    </form>
  </FormProvider>
  <BaseProgressBar
    class="mx-container mt-8 md:hidden"
    variant="decode"
    :active="decode.loading.value"
  />
  <BaseErrorList :errors="errors" />
</template>

<style scoped></style>
