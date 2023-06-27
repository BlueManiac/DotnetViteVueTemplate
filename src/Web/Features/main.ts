import 'bootstrap/scss/bootstrap.scss'
import './main.scss'

import 'bootstrap'

import '../Util/Client/array'

import { apiUrl, applicationName } from './info'
import { useApi } from '../Util/Client/fetch'
import { useSignalr } from '../Util/Client/signalr'

declare global {
  var api: ReturnType<typeof useApi> & { signalr: typeof useSignalr }
}

window.api = {
  ...useApi({ apiUrl }),
  signalr: (url) => useSignalr(apiUrl + url)
}

import { setPreferredTheme } from '../Components/ColorSchemes/color-schemes'

setPreferredTheme();

import { createApp } from 'vue'
import { Router, title } from './router'
import App from './App.vue'

createApp(App)
  .use(Router)
  .mount('#app')

import { useTitle } from '@vueuse/core'

useTitle(() => title.value
  ? title.value + " - " + applicationName
  : applicationName
)

if (import.meta.env.DEV) {
  const script = document.createElement('script');
  script.src = `${import.meta.env.VITE_API_URL}/_framework/aspnetcore-browser-refresh.js`
  document.body.appendChild(script)
}