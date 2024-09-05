<template>
  <div class="form-check" v-bind:class="{ 'd-print-none': !modelValue }">
    <input class="form-check-input" type="checkbox" v-model="modelValue" :id>
    <label class="form-check-label" :for="id">
      <slot></slot>
    </label>
  </div>
</template>

<script setup lang="ts">
const modelValue = defineModel<boolean>()

const id = Math.random().toString(36).substring(2, 12)
</script>