<template>
  <div>
    <input type="datetime-local" v-model="date" class="form-control">
  </div>
</template>

<script setup lang="ts">
import { computed } from 'vue'

const modelValue = defineModel<Date | string>()

const date = computed({
  get: () => {
    if (!modelValue.value) {
      return null
    }

    const value = modelValue.value instanceof Date
      ? modelValue.value
      : new Date(modelValue.value)

    return value.toISOString().slice(0, 16)
  },
  set: (value: string) => {
    modelValue.value = value
  }
})
</script>