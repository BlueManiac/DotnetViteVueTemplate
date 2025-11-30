import { useLocalStorage } from '@vueuse/core'

type ThemeId = 'light' | 'dark' | 'auto'
type Theme = { id: ThemeId, name: string, icon: any }

export const themes: Theme[] = [{
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

const getPreferredThemeId = () => {
  return window.matchMedia('(prefers-color-scheme: dark)').matches
    ? 'dark'
    : 'light'
}

export const currentThemeId = useLocalStorage<ThemeId>('theme', getPreferredThemeId())
export const currentTheme = computed<Theme>(() => {
  return themes.find(x => x.id == currentThemeId.value)!
})

export const setTheme = (theme: ThemeId) => {
  currentThemeId.value = theme

  if (theme === 'auto' && window.matchMedia('(prefers-color-scheme: dark)').matches) {
    theme = 'dark'
  }

  document.documentElement.setAttribute('data-bs-theme', theme)
}

export const initializeTheme = () => {
  setTheme(currentThemeId.value)
}