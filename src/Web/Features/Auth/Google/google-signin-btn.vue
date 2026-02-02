<template>
  <a :href="loginUrl" class="btn btn-lg btn-primary">
    <CarbonLogoGoogle class="me-2" />
    Sign in with Google
  </a>
</template>

<script setup lang="ts">
import { inject } from 'vue'
import { AppConfig } from '/Features/AppConfig'

const { redirect } = defineProps<{
  redirect?: string
}>()

const config = inject(AppConfig)

const loginUrl = computed(() => {
  const url = new URL(`${config.apiUrl}/auth/google-login`, window.location.origin)

  if (redirect) {
    url.searchParams.set('redirect', redirect)
  }

  return url.toString()
})
</script>
