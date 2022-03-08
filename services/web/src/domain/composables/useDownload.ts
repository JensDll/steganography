export function useDownload(fileName: string) {
  const createAndDownload = (file: Blob) => {
    const a = document.createElement('a')

    a.href = URL.createObjectURL(file)
    a.download = fileName
    a.click()

    URL.revokeObjectURL(a.href)
  }

  return {
    file(file: Blob) {
      createAndDownload(file)
    },
    text(content: string, contentType: string) {
      const file = new Blob([content], { type: contentType })
      createAndDownload(file)
    }
  }
}
