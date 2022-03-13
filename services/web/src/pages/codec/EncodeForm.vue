<script setup lang="ts">
import { useValidation, type Field } from 'validierung'
import { ref, computed } from 'vue'

import { rules, api } from '~/domain'

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
      rules.withPrecondition(isTextMode)(rules.required('Enter a message'))
    ]
  },
  binaryData: {
    $value: [],
    $rules: [
      rules.withPrecondition(isBinaryMode)(
        rules.min(1)('Attach one or more files')
      )
    ]
  },
  coverImage: {
    $value: [],
    $rules: [rules.minMax(1, 1)('Attach a cover image for your message')]
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
  } catch (e) {
    console.log(e)
  }
}
</script>

<template>
  <AppSection>
    <form @submit.prevent="handleSubmit">
      <section class="py-8 container lg:py-12">
        <div>
          <label for="message">Secret message</label>
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
              placeholder="Your data will be hidden and encrypted"
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
          class="mt-6"
        />
      </section>
      <section class="bg-encode-50 py-4 dark:bg-encode-900">
        <div
          class="grid grid-cols-[1fr_auto] gap-x-8 container md:gap-x-12"
          :class="{ 'justify-between': loading }"
        >
          <AppProgressBar
            class="mr-12 w-full lg:w-2/3"
            variant="encode"
            :active="loading"
          />
          <AppButton
            type="submit"
            variant="encode"
            class="grid-area-[1/2/2/3]"
            :disabled="loading"
          >
            Encode
          </AppButton>
        </div>
      </section>
    </form>
  </AppSection>
</template>
