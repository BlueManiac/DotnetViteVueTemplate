<template>
  <div class="login-container d-flex flex-column justify-content-center align-items-center py-4">
    <h1 class="text-center mb-3 fw-bold text-primary">{{ config.applicationName }}</h1>
    <div class="col-12 col-lg-5 col-xl-3 px-3 px-lg-0">
      <div class="d-flex flex-column gap-3 bg-body-secondary p-4 rounded-3 shadow">
        <password-login-form v-if="authService.providers.value.includes('password')" @success="handleRedirect" />
        <google-signin-btn v-if="authService.providers.value.includes('google')" class="w-100" :redirect="route.query.redirect as string" />
        <microsoft-signin-btn v-if="authService.providers.value.includes('microsoft')" class="w-100" :redirect="route.query.redirect as string" />
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { inject, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { NotificationService } from '../../Infrastructure/Notifications/notifications'
import { AuthService, useAuthCallback } from '../AuthService'
import { AppConfig } from '/Util/AppConfig'

const config = inject(AppConfig.token)!
const authService = inject(AuthService.token)!
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
  background: linear-gradient(135deg, rgba(var(--bs-primary-rgb), 0.05) 0%, rgba(var(--bs-secondary-rgb), 0.05) 100%);
}
</style>