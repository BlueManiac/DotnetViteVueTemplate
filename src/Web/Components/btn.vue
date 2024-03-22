<template>
  <button :class="`btn btn-${theme}`">
    <slot />
  </button>
</template>

<script setup lang="ts">
const { theme = 'primary' } = defineProps<{
  theme?: string
}>()
</script>