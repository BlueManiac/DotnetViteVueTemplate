import 'bootstrap/dist/css/bootstrap.css'
import './main.css'

import 'bootstrap'

import '../Util/Client/array'

import { applicationName } from './info'
import { showErrorOverlay } from '../Util/Client/client-error-handler'
import { setPreferredTheme } from '../Components/ColorSchemes/color-schemes'

setPreferredTheme();

import { createApp } from 'vue'
import { Router, title } from './router'
import App from './App.vue'

const app = createApp(App)
  .use(Router)

app.config.errorHandler = (error) => {
  showErrorOverlay(error)
  console.error(error)
  return false
}
Router.onError((error) => {
  showErrorOverlay(error)
  console.error(error)
})

app.mount('#app')

import { useTitle } from '@vueuse/core'

useTitle(() => title.value
  ? title.value + " - " + applicationName
  : applicationName
)

if (import.meta.env.DEV) {
  window.addEventListener('error', ({ error }) => {
    showErrorOverlay(error)
  })

  import.meta.hot?.on('vite:beforeUpdate', () => {
    console.clear()
  })
  
  const script = document.createElement('script');
  script.src = `${import.meta.env.VITE_API_URL}/_framework/aspnetcore-browser-refresh.js`
  document.body.appendChild(script)
}