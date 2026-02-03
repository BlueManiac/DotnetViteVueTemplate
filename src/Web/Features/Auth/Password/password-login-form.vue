<template>
  <form class="d-flex flex-column gap-3" @submit.prevent="submit">
    <div class="form-floating">
      <input type="email" v-model="email" class="form-control" placeholder="name@example.com" ref="emailElement" required autocomplete="email">
      <label>Email address</label>
    </div>
    <div class="form-floating">
      <input type="password" v-model="password" class="form-control" placeholder="Password" required autocomplete="current-password">
      <label>Password</label>
    </div>
    <button class="w-100 btn btn-lg btn-primary" type="submit" :disabled="!valid">Sign in</button>
  </form>
</template>

<script setup lang="ts">
import { useFocus } from '@vueuse/core'
import { inject, watch } from 'vue'
import { AccessTokenResponse } from '../AuthService'
import { Profile } from '../Profile'
import { ApiService } from '/Features/ApiService'

const emit = defineEmits<{
  success: []
}>()

const api = inject(ApiService)
const profile = inject(Profile)

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
  const response = await api.post<AccessTokenResponse>('/auth/login', { username: email.value, password: password.value })
  profile.setAuthTokens(response)
  emit('success')
}
</script>
