<template>
  <a :href="loginUrl" class="btn btn-lg btn-outline-secondary">
    <CarbonLogoGoogle class="me-2" />
    Sign in with Google
  </a>
</template>

<script setup lang="ts">
import { inject } from 'vue'
import { AppConfig } from '/Features/AppConfig'

const config = inject(AppConfig)!

const loginUrl = computed(() => `${config.apiUrl}/auth/google-login`)
</script>
