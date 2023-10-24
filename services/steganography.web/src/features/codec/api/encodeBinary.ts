import { ref } from 'vue'

import {
  useDownload,
  useFetch,
  ApiError,
  API_ERROR_UNKNOWN,
  type ErrorResponse,
} from '~/composables'

const { post: doEncodeBinary } = useFetch('/codec/encode/binary')

export async function encodeBinary(coverImage: File, files: File[]) {
  encodeBinary.loading.value = true
  encodeBinary.abort = () => {
    doEncodeBinary.abort()
  }

  const formData = new FormData()
  formData.append('coverImage', coverImage)
  for (const file of files) {
    formData.append('', file.size.toString())
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
        throw new ApiError(errors.join('. '))
      }

      throw API_ERROR_UNKNOWN
    }
  } finally {
    encodeBinary.abort = () => {}
    encodeBinary.loading.value = false
  }
}

encodeBinary.abort = () => {}
encodeBinary.loading = ref(false)
