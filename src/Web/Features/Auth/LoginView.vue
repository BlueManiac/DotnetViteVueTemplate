<template>
  <form class="col-lg-3" @submit.prevent="submit()">
    <h3 class="fw-normal">Please sign in</h3>
    <div class="form-floating">
      <input type="email" v-model="email" class="form-control" placeholder="name@example.com" ref="emailElement" required autocomplete="off">
      <label>Email address</label>
    </div>
    <div class="form-floating">
      <input type="password" v-model="password" class="form-control" placeholder="Password" required>
      <label>Password</label>
    </div>
    <button class="w-100 btn btn-lg btn-primary" type="submit" :disabled="!valid">Sign in</button>
  </form>
</template>

<script setup lang="ts">
import { useFocus } from '@vueuse/core'
import { login } from './AuthenticationService'
import { useRouter } from 'vue-router'
import { watch } from 'vue'

const emailElement = ref<HTMLInputElement | null>()
useFocus(emailElement, { initialValue: true })

const email = ref('')
const password = ref('')

const validEmail = ref<boolean>(false)
watch(email, () => {
  validEmail.value = emailElement.value.validity.valid
})
const valid = computed(() => {
  return validEmail.value && password.value
})

const router = useRouter()
const submit = async () => {
  await login(email.value, password.value)

  router.push('/')
}
</script>

<style scoped>
form>* {
  margin-bottom: 1rem
}
</style>