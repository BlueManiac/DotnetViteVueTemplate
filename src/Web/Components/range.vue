<template>
  <div>
    <input type="range" class="form-range" v-model="modelValue" :min :max>
  </div>
</template>

<script setup lang="ts">
const modelValue = defineModel<number | string>()

defineProps<{
  min: number | string,
  max: number | string
}>()
</script>