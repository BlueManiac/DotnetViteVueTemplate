<template>
  <div class="form-check">
    <input class="form-check-input" type="radio" v-model="modelValue" :value :id>
    <label class="form-check-label" :for="id">
      <slot></slot>
    </label>
  </div>
</template>

<script setup lang="ts">
import { defineModel } from 'vue'

defineProps<{
  value: unknown
}>()

const modelValue = defineModel<unknown>()

const id = Math.random().toString(36).substring(2, 12)
</script>