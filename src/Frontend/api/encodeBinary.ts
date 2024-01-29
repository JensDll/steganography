export async function encodeBinary(coverImage: File, files: File[]) {
  const abortController = new AbortController()

  encodeText.abort = () => {
    abortController.abort()
  }
  encodeText.pending.value = true

  const formData = new FormData()
  formData.append('coverImage', coverImage)
  for (const file of files) {
    formData.append('', file.size.toString())
    formData.append('', file)
  }

  try {
    const result = await $fetch('/api/v1/codec/encode/binary', {
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
encodeBinary.abort = () => {}
encodeBinary.pending = ref(false)
