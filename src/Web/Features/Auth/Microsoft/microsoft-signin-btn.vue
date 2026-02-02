<template>
  <a :href="loginUrl" class="btn btn-lg btn-primary">
    <MdiMicrosoft class="me-2" />
    Sign in with Microsoft
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
  const url = new URL(`${config.apiUrl}/auth/microsoft-login`, window.location.origin)

  if (redirect) {
    url.searchParams.set('redirect', redirect)
  }

  return url.toString()
})
</script>
