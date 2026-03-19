<template>
  <form class="d-flex flex-column gap-3" v-validate @submit="submit">
    <input-text v-model="email" type="email" required autocomplete="email" autofocus floating>Email address</input-text>
    <input-text v-model="password" type="password" required autocomplete="current-password" floating>Password</input-text>
    <btn class="w-100 btn-lg" type="submit" :disabled="!valid">Sign in</btn>
  </form>
</template>

<script setup lang="ts">
import { inject } from 'vue'
import { AccessTokenResponse } from '../../AuthService'
import { Profile } from '../../Profile'
import { ApiService } from '/ApiService'
import { useFormValidation } from '/Components/Validation/useFormValidation'

const emit = defineEmits<{
  success: []
}>()

const api = inject(ApiService.token)!
const profile = inject(Profile.token)!

const { vValidate, valid } = useFormValidation()

const email = ref('')
const password = ref('')

const submit = async () => {
  const response = await api.post<AccessTokenResponse>('/auth/login', { username: email.value, password: password.value }, { auth: false })
  profile.setAuthTokens(response)
  emit('success')
}
</script>
