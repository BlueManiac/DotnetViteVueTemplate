import 'bootstrap/scss/bootstrap.scss'
import './main.scss'

import { createApp } from 'vue'
import 'bootstrap'

import '../Util/Client/array'
import '../Util/Client/fetch'
import './info'

import App from './App.vue'
import { Router } from './router'

createApp(App)
  .use(Router)
  .mount('#app')