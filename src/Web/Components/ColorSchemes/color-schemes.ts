import { shallowRef } from 'vue'

type Theme = { id: string, name: string, icon: any }

export const themes: Theme[] = [{
  id: 'light',
  name: 'Light',
  icon: IconMdiLightbulbOn
}, {
  id: 'dark',
  name: 'Dark',
  icon: IconMdiMoonWaningCrescent
}, {
  id: 'auto',
  name: 'Auto',
  icon: IconMdiCircleHalfFull
}]

export const currentTheme = shallowRef<Theme>(null)

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