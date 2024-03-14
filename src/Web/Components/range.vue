<template>
  <div>
    <input type="range" class="form-range" v-model="modelValue" :min="min" :max="max">
  </div>
</template>

<script setup lang="ts">
defineProps<{
  min: number | string,
  max: number | string
}>()

const modelValue = defineModel<number | string>()
</script>