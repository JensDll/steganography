import { ref } from 'vue'

export function useApi() {
  const loading = ref(false)
  const base = 'https://localhost:5001/api'

  return {
    loading,
    api: {
      async encodeText(image: File, message: string) {
        const formData = new FormData()
        formData.append('coverImage', image)
        formData.append('message', message)

        console.log(formData)

        const request: RequestInit = {
          method: 'POST',
          body: formData
        }

        loading.value = true

        const response = await fetch(base + '/codec/encode/text', request)
        console.log(response)
        const data = await response.text()
        console.log(data)

        loading.value = false
      }
    }
  }
}
