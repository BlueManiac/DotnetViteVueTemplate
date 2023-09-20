import 'bootstrap/dist/css/bootstrap.css'
import './main.css'

import 'bootstrap'

import '../Util/Client/array'

import { apiUrl, applicationName } from './info'
import { useApi } from '../Util/Client/fetch'
import { useSignalr } from '../Util/Client/signalr'
import { showErrorOverlay } from '../Util/Client/client-error-handler'

declare global {
  var api: ReturnType<typeof useApi> & {
    url: string,
    signalr: typeof useSignalr
  }
}

window.api = {
  url: apiUrl,
  ...useApi({ apiUrl }),
  signalr: (url) => useSignalr(apiUrl + url)
}

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