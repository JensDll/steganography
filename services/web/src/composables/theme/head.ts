switch (localStorage.theme) {
  case 'light':
    document.documentElement.classList.remove('dark')
    break
  case 'dark':
    document.documentElement.classList.add('dark')
    break
  default:
    localStorage.setItem('theme', 'system')
    if (window.matchMedia('(prefers-color-scheme: dark)').matches) {
      document.documentElement.classList.add('dark')
    } else {
      document.documentElement.classList.remove('dark')
    }
}

export {}
