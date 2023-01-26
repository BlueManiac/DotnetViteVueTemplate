<template>
  <button class="btn" v-bind:class="`btn-${theme}`">
    <slot />
  </button>
</template>

<script setup>
  defineProps({
    theme: {
      type: String,
      default: 'primary'
    }
  })
</script>