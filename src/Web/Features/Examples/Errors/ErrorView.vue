<template>
  <div class="d-flex gap-2">
    <btn @click="clientError()">Trigger client error</btn>
    <btn @click="serverError()">Trigger server error</btn>
  </div>
</template>

<script setup>
import { api } from '/Features/api'

const clientError = () => {
  throw new Error('client error')
}
const serverError = async () => {
  await api.get('/error')
}
</script>