<template>
  Should not be seen
</template>

<script setup>
import { api } from '/Features/api'

await api.get('/error')
</script>