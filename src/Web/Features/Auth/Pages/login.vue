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
import { inject } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { AppConfig } from '../../AppConfig'
import { AuthService, useAuthCallback } from '../AuthService'

const config = inject(AppConfig)
const authService = inject(AuthService)
const router = useRouter()
const route = useRoute()

const handleRedirect = () => {
  const redirect = route.query.redirect as string
  const redirectPath = redirect && redirect.startsWith('/') ? redirect : '/'
  router.push(redirectPath)
}

useAuthCallback(handleRedirect)

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