<template>
  <span>
    {{ text }}
    <slot />
  </span>
</template>

<script setup lang="ts">
const { rotation } = defineProps<{
  text: string,
  rotation: number | string
}>()

const rotationInDegrees = computed(() => rotation + 'deg')
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