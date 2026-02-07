import 'bootstrap/dist/css/bootstrap.css'
import './main.css'

import 'bootstrap'

import '../Util/Client/array'

import { useTitle } from '@vueuse/core'
import { initializeTheme } from '../Components/ColorThemes/color-themes'
import { NotificationService } from '../Components/Notifications/notifications'
import { ApiService } from './ApiService'
import { AppConfig } from './AppConfig'
import { AuthService } from './Auth/AuthService'
import { Profile } from './Auth/Profile'
import { TokenValidator } from './Auth/TokenValidator'
import { HealthService } from './Health/HealthService'

initializeTheme()

import { createApp } from 'vue'
import App from './App.vue'
import { Router, setupAuthGuard, title } from './router'

const config = new AppConfig()
const profile = new Profile()
const notifications = new NotificationService()
const health = new HealthService(config, notifications)
const tokenValidator = new TokenValidator(
  () => profile.expiresAt.value,
  () => profile.isLoggedIn.value
)
const api = new ApiService(tokenValidator, config, profile, health)
const authService = new AuthService(tokenValidator, api, profile, notifications)

setupAuthGuard(profile, tokenValidator)

const app = createApp(App)
  .use(Router)
  .provide(AppConfig, config)
  .provide(Profile, profile)
  .provide(HealthService, health)
  .provide(TokenValidator, tokenValidator)
  .provide(NotificationService, notifications)
  .provide(ApiService, api)
  .provide(AuthService, authService)

Router.isReady().then(() => {
  app.mount('#app')
})

useTitle(() => title.value
  ? title.value + " - " + config.applicationName
  : config.applicationName
)

if (import.meta.env.DEV) {
  import.meta.hot?.on('vite:beforeUpdate', () => {
    console.clear()
  })

  const script = document.createElement('script')
  script.src = `${import.meta.env.VITE_BACKEND_URL}/_framework/aspnetcore-browser-refresh.js`
  document.body.appendChild(script)
}