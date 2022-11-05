<script setup lang="ts">
import { type Field, useValidation } from 'validierung'
import { computed, ref, type VNode } from 'vue'

import { encodeText, encodeBinary } from '../api'
import { withPrecondition, required, min, minMax } from '~/common/rules'
import {
  BaseErrorListAdd,
  BaseErrorListClear
} from '~/components/base/BaseErrorList.vue'
import { ApiError } from '~/composables/useFetch'

type FormData = {
  textData: Field<string>
  binaryData: Field<File[]>
  coverImage: Field<File[]>
}

const messageMode = ref<'text' | 'binary'>('text')
const isTextMode = computed(() => messageMode.value === 'text')
const isBinaryMode = computed(() => messageMode.value === 'binary')

const { form, validateFields } = useValidation<FormData>({
  textData: {
    $value: '',
    $rules: [withPrecondition(isTextMode)(required('Please enter a message'))]
  },
  binaryData: {
    $value: [],
    $rules: [
      withPrecondition(isBinaryMode)(min(1)('Please attach one or more files'))
    ]
  },
  coverImage: {
    $value: [],
    $rules: [minMax(1, 1)('Please attach a cover image')]
  }
})

const errors = ref<VNode[]>([])
const loading = computed(
  () => encodeText.loading.value || encodeBinary.loading.value
)
const abort = () => {
  encodeText.abort()
  encodeBinary.abort()
}

async function handleSubmit() {
  try {
    const formData = await validateFields()
    if (messageMode.value === 'text') {
      await encodeText(formData.coverImage[0], formData.textData)
    } else {
      await encodeBinary(formData.coverImage[0], formData.binaryData)
    }
    BaseErrorListClear(errors)
  } catch (error) {
    if (error instanceof ApiError) {
      BaseErrorListAdd(errors, error.vNode)
    }
  }
}
</script>

<template>
  <FormProvider>
    <form @submit.prevent="handleSubmit">
      <section class="container py-8 lg:py-12">
        <div>
          <label for="message" class="mb-0">Secret message</label>
          <div class="mb-2 flex">
            <label class="mb-0 flex cursor-pointer items-center font-normal">
              <input
                v-model="messageMode"
                class="mr-1 cursor-pointer"
                :class="{ error: isTextMode && form.textData.$hasError }"
                type="radio"
                name="messageType"
                value="text"
              />
              Text data
            </label>
            <label
              class="ml-3 mb-0 flex cursor-pointer items-center font-normal"
            >
              <input
                v-model="messageMode"
                class="mr-1 cursor-pointer"
                :class="{ error: isBinaryMode && form.binaryData.$hasError }"
                type="radio"
                name="messageType"
                value="binary"
              />
              Binary data
            </label>
          </div>
          <template v-if="isTextMode">
            <textarea
              id="message"
              v-model="form.textData.$value"
              placeholder="Your data will be hidden and encrypted. Nothing gets logged or saved anywhere"
              class="max-h-[24rem] min-h-[8rem] w-full"
              :class="{ error: form.textData.$hasError }"
            />
            <FormErrors :errors="form.textData.$errors" />
          </template>
          <FormFileInput
            v-else
            id="message"
            v-model="form.binaryData.$value"
            :errors="form.binaryData.$errors"
            multiple
          />
        </div>
        <FormFileInput
          v-model="form.coverImage.$value"
          :errors="form.coverImage.$errors"
          label="Attach a cover image"
          accept="image/*"
          class="mt-6"
        />
      </section>
      <section class="bg-encode-100 py-4 dark:bg-encode-800">
        <div
          class="container grid grid-cols-[1fr_auto] gap-x-8 md:gap-x-12"
          :class="{ 'justify-between': loading }"
        >
          <BaseProgressBar
            class="mr-12 hidden w-full md:grid lg:w-2/3"
            variant="encode"
            :active="loading"
          />
          <div class="flex grid-area-[1/2/2/3]">
            <BaseButton class="mr-4" @click="abort()">Cancel</BaseButton>
            <BaseButton type="submit" variant="encode" :disabled="loading">
              Encode
            </BaseButton>
          </div>
        </div>
      </section>
    </form>
  </FormProvider>
  <BaseProgressBar
    class="mx-container mt-8 md:hidden"
    variant="encode"
    :active="loading"
  />
  <BaseErrorList :errors="errors" />
</template>

<style scoped></style>
