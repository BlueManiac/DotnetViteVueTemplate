import 'bootstrap/dist/css/bootstrap.css'
import './main.css'

import 'bootstrap'

import '../Util/Client/array'

import { setPreferredTheme } from '../Components/ColorThemes/color-themes'
import { applicationName } from './info'

import { useTitle } from '@vueuse/core'
import { AuthenticationService } from './Auth/AuthenticationService'

setPreferredTheme()

import { createApp } from 'vue'
import App from './App.vue'
import { Router, title } from './router'

const app = createApp(App)
  .use(Router)
  .provide(AuthenticationService)

app.mount('#app')

useTitle(() => title.value
  ? title.value + " - " + applicationName
  : applicationName
)

if (import.meta.env.DEV) {
  import.meta.hot?.on('vite:beforeUpdate', () => {
    console.clear()
  })

  const script = document.createElement('script')
  script.src = `${import.meta.env.VITE_API_URL}/_framework/aspnetcore-browser-refresh.js`
  document.body.appendChild(script)
}