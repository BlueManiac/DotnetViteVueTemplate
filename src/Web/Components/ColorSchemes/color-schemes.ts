import { shallowRef } from 'vue'

export const themes = [{
  id: 'light',
  name: 'Light',
  icon: MdiLightbulbOn
}, {
  id: 'dark',
  name: 'Dark',
  icon: MdiMoonWaningCrescent
}, {
  id: 'auto',
  name: 'Auto',
  icon: MdiCircleHalfFull
}]

export const currentTheme = shallowRef(null)

export const setTheme = (theme: string) => {
  currentTheme.value = themes.find(x => x.id == theme)
  localStorage.setItem('theme', theme)

  if (theme === 'auto' && window.matchMedia('(prefers-color-scheme: dark)').matches) {
    theme = 'dark'
  }

  document.documentElement.setAttribute('data-bs-theme', theme)
}

const getPreferredTheme = () => {
  const storedTheme = localStorage.getItem('theme')

  if (storedTheme) {
    return storedTheme
  }

  return window.matchMedia('(prefers-color-scheme: dark)').matches ? 'dark' : 'light'
}

export const setPreferredTheme = () => setTheme(getPreferredTheme())