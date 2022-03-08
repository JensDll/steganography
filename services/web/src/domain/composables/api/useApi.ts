import { ref } from 'vue'
import { useDownload } from '../useDownload'
import { createFetch } from './createFetch'

const { useFetch } = createFetch('https://localhost:5001')

export function useApi() {
  const loading = ref(false)

  return {
    loading,
    codec: {
      async encodeText(coverImage: File, message: string) {
        loading.value = true

        const formData = new FormData()
        formData.append('coverImage', coverImage)
        formData.append('message', message)

        const { response } = await useFetch('/codec/encode/text').post({
          body: formData
        })

        if (response && response.ok) {
          const zip = await response.blob()
          useDownload('secret.zip').file(zip)
        }

        loading.value = false
      },

      async encodeBinary(coverImage: File, files: File[]) {
        loading.value = true

        const formData = new FormData()
        formData.append('coverImage', coverImage)
        for (let i = 0; i < files.length; ++i) {
          formData.append(i.toString(), files[i])
        }

        const { response } = await useFetch('/codec/encode/binary').post({
          body: formData
        })

        if (response && response.ok) {
          const zip = await response.blob()
          useDownload('secret.zip').file(zip)
        }

        loading.value = false
      },

      async decode(coverImage: File, key: string) {
        loading.value = true

        const formData = new FormData()
        formData.append('coverImage', coverImage)
        formData.append('key', key)

        const { response, responseType } = await useFetch('/codec/decode').post(
          {
            body: formData
          }
        )

        if (response && response.ok) {
          if (responseType === 'text') {
            const text = await response.text()
            console.log(text)
          } else if (responseType === 'blob') {
            const zip = await response.blob()
            useDownload('result.zip').file(zip)
          }
        }

        loading.value = false
      }
    }
  }
}
