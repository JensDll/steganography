import { ref } from 'vue'

import { useDownload, useFetch, ApiError } from '~/composables'

const { post: doDecode } = useFetch('/codec/decode')

export async function decode(coverImage: File, key: string) {
  decode.loading.value = true
  decode.abort = () => {
    doDecode.abort()
  }

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
      throw new ApiError('Decoding failed. Maybe your key is not valid')
    }
  } finally {
    decode.abort = () => {}
    decode.loading.value = false
  }
}

decode.abort = () => {}
decode.loading = ref(false)
