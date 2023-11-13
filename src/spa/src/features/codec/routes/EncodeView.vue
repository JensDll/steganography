<script setup lang="ts">
import { type Field, useValidation } from 'validierung'
import { computed, ref, type VNode } from 'vue'

import { encodeText, encodeBinary } from '../api'
import { withPrecondition, required, min, minMax } from '~/common/rules'
import {
  BaseErrorListAdd,
  BaseErrorListClear,
} from '~/components/base/BaseErrorList.vue'
import { ApiError } from '~/composables'

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
    $rules: [withPrecondition(isTextMode)(required('Please enter a message'))],
  },
  binaryData: {
    $value: [],
    $rules: [
      withPrecondition(isBinaryMode)(min(1)('Please attach one or more files')),
    ],
  },
  coverImage: {
    $value: [],
    $rules: [minMax(1, 1)('Please attach a cover image')],
  },
})

const errors = ref<VNode[]>([])
const loading = computed(
  () => encodeText.loading.value || encodeBinary.loading.value,
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
  <form @submit.prevent="handleSubmit">
    <section>
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
          <label class="mb-0 ml-3 flex cursor-pointer items-center font-normal">
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
    <section class="mt-10 flex items-center justify-end">
      <LoadingIndicator class="mr-6 text-highlight-encode" :loading="loading" />
      <BaseButton class="mr-4" @click="abort()">Cancel</BaseButton>
      <BaseButton type="submit" variant="encode" :disabled="loading">
        Encode
      </BaseButton>
    </section>
  </form>
  <BaseErrorList :errors="errors" />
</template>

<style scoped></style>
