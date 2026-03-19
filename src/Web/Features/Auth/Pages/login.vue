<template>
  <div class="login-container d-flex flex-column align-items-center">
    <h1 class="text-center fw-bold text-primary mb-3">{{ config.applicationName }}</h1>
    <div class="col-12 col-lg-5 col-xl-3 px-3 px-lg-0">
      <div class="d-flex flex-column gap-3 bg-body-secondary p-4 rounded-3 shadow">
        <template v-if="health.backendReady.value">
          <password-login-form v-if="authService.providers.value.includes('password')" @success="handleRedirect" />
          <google-signin-btn v-if="authService.providers.value.includes('google')" class="w-100" :redirect="route.query.redirect as string" />
          <microsoft-signin-btn v-if="authService.providers.value.includes('microsoft')" class="w-100" :redirect="route.query.redirect as string" />
        </template>
        <div v-else class="d-flex flex-column align-items-center gap-2 py-3 text-body-secondary">
          <div class="spinner-border text-secondary" role="status">
            <span class="visually-hidden">Loading...</span>
          </div>
          <small>Loading...</small>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { inject, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { HealthService } from '../../Infrastructure/Health/HealthService'
import { NotificationService } from '../../Infrastructure/Notifications/notifications'
import { AuthService, useAuthCallback } from '../AuthService'
import { AppConfig } from '/Util/AppConfig'

const config = inject(AppConfig.token)!
const authService = inject(AuthService.token)!
const health = inject(HealthService.token)!
const notifications = inject(NotificationService.token)!

const router = useRouter()
const route = useRoute()

const handleRedirect = () => {
  const redirect = route.query.redirect as string
  const redirectPath = redirect && redirect.startsWith('/') ? redirect : '/'
  router.push(redirectPath)
}

useAuthCallback(handleRedirect)

// Display authentication errors as notifications
onMounted(() => {
  const errorMessage = route.query.errorMessage as string | undefined

  if (errorMessage) {
    notifications.notify({
      type: 'error',
      title: 'Authentication Failed',
      message: errorMessage,
      persistent: true
    })

    // Clean up URL query parameters
    const { error, errorMessage: _, ...remainingQuery } = route.query
    router.replace({ query: remainingQuery })
  }
})

definePage({
  meta: {
    display: 'full',
    auth: false
  }
})
</script>

<style scoped>
.login-container {
  padding-top: 25vh;
  background: linear-gradient(135deg, rgba(var(--bs-primary-rgb), 0.05) 0%, rgba(var(--bs-secondary-rgb), 0.05) 100%);
}
</style>