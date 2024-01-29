export async function encodeText(coverImage: File, message: string) {
  const abortController = new AbortController()

  encodeText.abort = () => {
    abortController.abort()
  }
  encodeText.pending.value = true

  const formData = new FormData()
  formData.append('coverImage', coverImage)
  formData.append('message', message)

  try {
    const result = await $fetch('/api/v1/codec/encode/text', {
      method: 'POST',
      body: formData,
      signal: abortController.signal,
    })

    console.log(result)
  } finally {
    encodeText.abort = () => {}
    encodeText.pending.value = false
  }
}
encodeText.abort = () => {}
encodeText.pending = ref(false)
