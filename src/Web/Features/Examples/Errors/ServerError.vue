<template>
  Should not be seen
</template>

<script setup lang="ts">
import { api } from '/Features/api'

await api.get('/error')
</script>