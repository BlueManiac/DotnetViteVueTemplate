import { shallowRef } from "vue";

export const themes = [{
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
}];

export const currentTheme = shallowRef(null);

export const setTheme = theme => {
  localStorage.setItem('theme', theme)
  currentTheme.value = themes.find(x => x.id == theme)

  if (theme === 'auto' && window.matchMedia('(prefers-color-scheme: dark)').matches) {
    document.documentElement.setAttribute('data-bs-theme', 'dark')
  } else {
    document.documentElement.setAttribute('data-bs-theme', theme)
  }
}

const getPreferredTheme = () => {
  const storedTheme = localStorage.getItem('theme')

  if (storedTheme) {
    return storedTheme
  }

  return window.matchMedia('(prefers-color-scheme: dark)').matches ? 'dark' : 'light'
}

setTheme(getPreferredTheme())