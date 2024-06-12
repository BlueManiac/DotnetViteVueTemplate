import 'bootstrap/dist/css/bootstrap.css'
import './main.css'

import 'bootstrap'

import '../Util/Client/array'

import { setPreferredTheme } from '../Components/ColorSchemes/color-schemes'
import { applicationName } from './info'

setPreferredTheme()

import { createApp } from 'vue'
import App from './App.vue'
import { Router, title } from './router'

const app = createApp(App)
  .use(Router)

app.mount('#app')

import { useTitle } from '@vueuse/core'

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