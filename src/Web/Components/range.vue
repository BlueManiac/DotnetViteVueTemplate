<template>
  <input type="range" class="form-range" v-model="modelValue" :min :max>
</template>

<script setup lang="ts">
import { defineModel, defineProps } from 'vue'

const modelValue = defineModel<number | string>()

defineProps<{
  min: number | string,
  max: number | string
}>()
</script>