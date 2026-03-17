import 'bootstrap/dist/css/bootstrap.css'
import './main.css'

import 'bootstrap'

import './Util/Client/array'

import { until, useTitle } from '@vueuse/core'
import { ApiService } from './ApiService'
import { initializeTheme } from './Components/ColorThemes/color-themes'
import { AuthService } from './Features/Auth/AuthService'
import { Profile } from './Features/Auth/Profile'
import { TokenValidator } from './Features/Auth/TokenValidator'
import { HealthService } from './Features/Infrastructure/Health/HealthService'
import { LocalizationService } from './Features/Infrastructure/Localization/LocalizationService'
import { NotificationService } from './Features/Infrastructure/Notifications/notifications'
import { AppConfig } from './Util/AppConfig'

initializeTheme()

import { createApp } from 'vue'
import App from './App.vue'
import { Router, setupAuthGuard, title } from './Util/router'

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
const localization = new LocalizationService()

setupAuthGuard(profile, tokenValidator)

const app = createApp(App)
  .use(Router)
  .provide(AppConfig.token, config)
  .provide(Profile.token, profile)
  .provide(HealthService.token, health)
  .provide(TokenValidator.token, tokenValidator)
  .provide(NotificationService.token, notifications)
  .provide(ApiService.token, api)
  .provide(AuthService.token, authService)
  .provide(LocalizationService.token, localization)

localization.registerGlobalProperty(app, '$t')

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

  // Wait for backend to be ready before loading browser refresh script
  until(health.backendReady).toBe(true).then(() => {
    const script = document.createElement('script')
    script.src = `${import.meta.env.VITE_BACKEND_URL}/_framework/aspnetcore-browser-refresh.js`
    document.body.appendChild(script)
  })
}