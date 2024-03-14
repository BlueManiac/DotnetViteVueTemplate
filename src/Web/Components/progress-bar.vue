<template>
  <div class="progress">
    <div class="progress-bar" role="progressbar" v-bind:aria-valuenow="modelValue" v-bind:aria-valuemin="min" v-bind:aria-valuemax="max"></div>
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