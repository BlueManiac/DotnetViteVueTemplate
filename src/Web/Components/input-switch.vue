<template>
  <div class="form-check form-switch">
    <input v-model="modelValue" class="form-check-input" type="checkbox" role="switch" :id>
    <label class="form-check-label" :for="id">
      <slot></slot>
    </label>
  </div>
</template>

<script setup lang="ts">
import { useId } from 'vue'

const modelValue = defineModel<boolean>()
const id = useId()
</script>