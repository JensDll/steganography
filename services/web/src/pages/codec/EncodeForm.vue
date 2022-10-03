<script setup lang="ts">
import { type Field, useValidation } from 'validierung'
import { computed, ref } from 'vue'

import { api, rules } from '~/domain'

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
    $rules: [
      rules.withPrecondition(isTextMode)(
        rules.required('Please enter a message')
      )
    ]
  },
  binaryData: {
    $value: [],
    $rules: [
      rules.withPrecondition(isBinaryMode)(
        rules.min(1)('Please attach one or more files')
      )
    ]
  },
  coverImage: {
    $value: [],
    $rules: [rules.minMax(1, 1)('Please attach a cover image')]
  }
})

const { loading, encodeText, encodeBinary } = api.codec()

async function handleSubmit() {
  try {
    const formData = await validateFields()
    if (messageMode.value === 'text') {
      await encodeText(formData.coverImage[0], formData.textData)
    } else {
      await encodeBinary(formData.coverImage[0], formData.binaryData)
    }
  } catch {}
}
</script>

<template>
  <FormProvider>
    <form @submit.prevent="handleSubmit">
      <section class="container py-8 lg:py-12">
        <div>
          <label for="message" class="mb-0">Secret message</label>
          <div class="mb-2 flex items-center">
            <input
              id="text-mode"
              v-model="messageMode"
              :class="{ error: isTextMode && form.textData.$hasError }"
              type="radio"
              name="messageType"
              value="text"
            />
            <label for="text-mode" class="mb-0 ml-1 font-normal">
              Text data
            </label>
            <input
              id="binary-mode"
              v-model="messageMode"
              :class="{ error: isBinaryMode && form.binaryData.$hasError }"
              type="radio"
              name="messageType"
              class="ml-3"
              value="binary"
            />
            <label for="binary-mode" class="mb-0 ml-1 font-normal">
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
          <VProgressBar
            class="mr-12 w-full lg:w-2/3"
            variant="encode"
            :active="loading"
          />
          <VButton
            type="submit"
            variant="encode"
            class="grid-area-[1/2/2/3]"
            :disabled="loading"
          >
            Encode
          </VButton>
        </div>
      </section>
    </form>
  </FormProvider>
</template>
