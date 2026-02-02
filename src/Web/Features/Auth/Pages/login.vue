<template>
  <form class="col-lg-5 col-xl-3" @submit.prevent="submit()">
    <h1 class="text-center mb-4">{{ config.applicationName }}</h1>
    <div class="form-floating">
      <input type="email" v-model="email" class="form-control" placeholder="name@example.com" ref="emailElement" required autocomplete="email">
      <label>Email address</label>
    </div>
    <div class="form-floating">
      <input type="password" v-model="password" class="form-control" placeholder="Password" required autocomplete="current-password">
      <label>Password</label>
    </div>
    <button class="w-100 btn btn-lg btn-primary" type="submit" :disabled="!valid">Sign in</button>

    <google-signin-btn v-if="authService.providers.value.includes('google')" class="w-100" :redirect="route.query.redirect as string" />
    <microsoft-signin-btn v-if="authService.providers.value.includes('microsoft')" class="w-100" :redirect="route.query.redirect as string" />
  </form>
</template>

<script setup lang="ts">
import { useFocus } from '@vueuse/core'
import { inject, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { AppConfig } from '../../AppConfig'
import { AuthService, useAuthCallback } from '../AuthService'

const config = inject(AppConfig)
const authService = inject(AuthService)
const router = useRouter()
const route = useRoute()

const emailElement = ref<HTMLInputElement | null>()
useFocus(emailElement, { initialValue: true })

const email = ref('')
const password = ref('')

const validEmail = ref<boolean>(false)
watch(email, () => {
  validEmail.value = emailElement.value?.validity.valid ?? false
})
const valid = computed(() => {
  return validEmail.value && password.value
})

const submit = async () => {
  await authService.login(email.value, password.value)
  handleRedirect()
}

const handleRedirect = () => {
  const redirect = route.query.redirect as string
  const redirectPath = redirect && redirect.startsWith('/') ? redirect : '/'
  router.push(redirectPath)
}

useAuthCallback(handleRedirect)

definePage({
  meta: {
    centered: true,
    auth: false
  }
})
</script>

<style scoped>
form > * {
  margin-bottom: 1rem
}
</style>