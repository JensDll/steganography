import { computed, ref } from 'vue'

import { useDownload, useFetch } from '~/domain'
import { ApiError, API_ERROR_GENERIC } from '~/domain/api/apiError'
import type { ErrorResponse } from '~/domain/api/types'

export function codec() {
  const abort = {
    value: () => {}
  }

  const { post: doEncodeText } = useFetch('/codec/encode/text')
  const { post: doEncodeBinary } = useFetch('/codec/encode/binary')
  const { post: doDecode } = useFetch('/codec/decode')

  async function encodeText(coverImage: File, message: string) {
    encodeText.loading.value = true
    const abortThis = () => {
      doEncodeText.abort()
    }
    encodeText.abort = abortThis
    abort.value = abortThis

    const formData = new FormData()
    formData.append('coverImage', coverImage)
    formData.append('message', message)

    try {
      const { response } = await doEncodeText({ body: formData })

      if (response.ok) {
        const zip = await response.blob()
        useDownload('secret.zip').file(zip)
      } else {
        const { errors } = (await response.json()) as ErrorResponse

        if (errors.length) {
          throw new ApiError(`${errors.join('. ')}.`)
        } else {
          throw API_ERROR_GENERIC
        }
      }
    } finally {
      abort.value = () => {}
      encodeText.abort = () => {}
      encodeText.loading.value = false
    }
  }
  encodeText.abort = () => {}
  encodeText.loading = ref(false)

  async function encodeBinary(coverImage: File, files: File[]) {
    encodeBinary.loading.value = true
    const abortThis = () => {
      doEncodeBinary.abort()
    }
    encodeBinary.abort = abortThis
    abort.value = abortThis

    const formData = new FormData()
    formData.append('coverImage', coverImage)
    for (const file of files) {
      formData.append('', file)
    }

    try {
      const { response } = await doEncodeBinary({ body: formData })

      if (response.ok) {
        const zip = await response.blob()
        useDownload('secret.zip').file(zip)
      } else {
        const { errors } = (await response.json()) as ErrorResponse

        if (errors.length) {
          throw new ApiError(`${errors.join('. ')}.`)
        } else {
          throw API_ERROR_GENERIC
        }
      }
    } finally {
      abort.value = () => {}
      encodeBinary.abort = () => {}
      encodeBinary.loading.value = false
    }
  }
  encodeBinary.abort = () => {}
  encodeBinary.loading = ref(false)

  async function decode(coverImage: File, key: string) {
    decode.loading.value = true
    const abortThis = () => {
      doDecode.abort()
    }
    decode.abort = abortThis
    abort.value = abortThis

    const formData = new FormData()
    formData.append('coverImage', coverImage)
    formData.append('key', key)

    try {
      const { response, responseType } = await doDecode({
        body: formData
      })

      if (response.ok) {
        if (responseType === 'text') {
          const text = await response.text()
          useDownload('result.txt').text(text)
        } else if (responseType === 'blob') {
          const zip = await response.blob()
          useDownload('result.zip').file(zip)
        }
      } else {
        throw new ApiError('Decoding failed. Maybe your key is not valid.')
      }
    } finally {
      abort.value = () => {}
      decode.abort = () => {}
      decode.loading.value = false
    }
  }
  decode.abort = () => {}
  decode.loading = ref(false)

  return {
    loading: computed(
      () =>
        encodeText.loading.value ||
        encodeBinary.loading.value ||
        decode.loading.value
    ),
    abort,
    encodeText,
    encodeBinary,
    decode
  }
}
