export function useDownload(fileName: string) {
  const createAndDownload = (file: Blob) => {
    const a = document.createElement('a')

    const url = URL.createObjectURL(file)
    a.href = url
    a.download = fileName
    a.click()

    URL.revokeObjectURL(url)
  }

  return {
    file(file: Blob) {
      createAndDownload(file)
    },
    text(content: string) {
      const file = new Blob([content], { type: 'text/plain' })
      createAndDownload(file)
    }
  }
}
