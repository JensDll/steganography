export function useScrollTo(id: string) {
  return () => {
    const target = document.getElementById(id)
    if (target) {
      target.scrollIntoView({
        behavior: 'smooth'
      })
    }
  }
}
