import { ref } from 'vue'

import { useDownload, useFetch } from '~/domain'

export function codec() {
  const loading = ref(false)

  return {
    loading,
    async encodeText(coverImage: File, message: string) {
      loading.value = true

      const formData = new FormData()
      formData.append('coverImage', coverImage)
      formData.append('message', message)

      const { isNetworkError, response } = await useFetch(
        '/codec/encode/text'
      ).post({
        body: formData
      })

      try {
        if (isNetworkError) {
          return
        }

        if (response.ok) {
          const zip = await response.blob()
          useDownload('secret.zip').file(zip)
        } else {
          throw await response.json()
        }
      } finally {
        loading.value = false
      }
    },

    async encodeBinary(coverImage: File, files: File[]) {
      loading.value = true

      const formData = new FormData()
      formData.append('coverImage', coverImage)
      for (const file of files) {
        formData.append('', file)
      }

      const { isNetworkError, response } = await useFetch(
        '/codec/encode/binary'
      ).post({
        body: formData
      })

      try {
        if (isNetworkError) {
          return
        }

        if (response.ok) {
          const zip = await response.blob()
          useDownload('secret.zip').file(zip)
        } else {
          throw await response.json()
        }
      } finally {
        loading.value = false
      }
    },

    async decode(coverImage: File, key: string) {
      loading.value = true

      const formData = new FormData()
      formData.append('coverImage', coverImage)
      formData.append('key', key)

      const { isNetworkError, response, responseType } = await useFetch(
        '/codec/decode'
      ).post({
        body: formData
      })

      try {
        if (isNetworkError) {
          return
        }

        if (response.ok) {
          if (responseType === 'text') {
            const text = await response.text()
            useDownload('result.txt').text(text)
          } else if (responseType === 'blob') {
            const zip = await response.blob()
            useDownload('result.zip').file(zip)
          }
        } else {
          throw await response.json()
        }
      } finally {
        loading.value = false
      }
    }
  }
}
