import { ref } from 'vue'

import { useFetch, useDownload } from '~/composables'
import {
  ApiError,
  API_ERROR_UNKNOWN,
  type ErrorResponse
} from '~/composables/useFetch'

const { post: doEncodeText } = useFetch('/codec/encode/text')

export async function encodeText(coverImage: File, message: string) {
  encodeText.loading.value = true
  encodeText.abort = () => {
    doEncodeText.abort()
  }

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
      }

      throw API_ERROR_UNKNOWN
    }
  } finally {
    encodeText.abort = () => {}
    encodeText.loading.value = false
  }
}
encodeText.abort = () => {}
encodeText.loading = ref(false)
