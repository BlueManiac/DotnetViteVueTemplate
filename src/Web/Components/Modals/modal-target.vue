<template>
  <template v-for="modal in modals">
    <component :is="modal" />
  </template>
</template>

<script setup lang="ts">
import { modals } from "./modal"
</script>