<template>
  <button class="btn btn-primary">
    <input type="file" ref="fileElement" @change="fileSelected()" />
    <slot />
  </button>
</template>

<script setup lang="ts">
import { defineModel, watch } from 'vue'

const modelValue = defineModel<File>()
const fileElement = ref<HTMLInputElement>(null)

watch(modelValue, () => {
  if (!modelValue.value) {
    fileElement.value.value = null
  }
})

const fileSelected = () => {
  const file = fileElement.value.files?.[0]
  if (file) {
    modelValue.value = file
  }
}
</script>

<style scoped>
.btn {
  position: relative;
  overflow: hidden;

  > input[type=file] {
    position: absolute;
    top: 0;
    right: 0;
    min-width: 100%;
    min-height: 100%;
    font-size: 100px;
    text-align: right;
    filter: alpha(opacity=0);
    opacity: 0;
    outline: none;
    background: white;
    cursor: inherit;
    display: block;
  }
}
</style>