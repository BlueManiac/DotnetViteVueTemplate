<template>
  <div class="progress">
    <div class="progress-bar" role="progressbar" :aria-valuenow="modelValue" :aria-valuemin="min" :aria-valuemax="max"></div>
  </div>
</template>

<script setup lang="ts">
const { modelValue, min = 0, max = 100 } = defineProps<{
  modelValue: number,
  min?: number | string,
  max: number | string,
}>()

const width = computed(() => modelValue / Number(max) * 100)
</script>

<style scoped>
.progress-bar {
  width: calc(v-bind(width) * 1%);
}
</style>