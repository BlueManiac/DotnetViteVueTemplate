<template>
  <span>
    {{text}}
    <slot />
  </span>
</template>

<script setup>
    import { computed } from 'vue'

    const props = defineProps({
        text: String,
        rotation: [Number, String]
    })

    const rotationInDegrees = computed(() => props.rotation + 'deg')
</script>

<style scoped>
  span {
    display: inline-block;
    transform: rotate(v-bind(rotationInDegrees));
    transform-origin: 50% 50%;
    position: relative;
    z-index: -1;
  }
</style>