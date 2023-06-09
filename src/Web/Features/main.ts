import 'bootstrap/scss/bootstrap.scss'
import './main.scss'

import { createApp } from 'vue'
import 'bootstrap'

import '../Util/Client/array'
import '../Util/Client/fetch'
import { applicationName } from './info'

import App from './App.vue'
import { Router, title } from './router'

import '../Components/ColorSchemes/color-schemes'

createApp(App)
  .use(Router)
  .mount('#app')

import { useTitle } from '@vueuse/core'

useTitle(() => title.value
  ? title.value + " - " + applicationName
  : applicationName
)