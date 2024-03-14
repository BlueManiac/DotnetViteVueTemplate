<template>
  <button :class="classes">
    <slot />
  </button>
</template>

<script setup lang="ts">
const { theme = 'primary' } = defineProps<{
  theme?: string
}>()

const classes = computed(() => `btn btn-${theme}`)
</script>