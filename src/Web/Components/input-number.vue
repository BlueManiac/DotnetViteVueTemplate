<template>
  <div>
    <input type="number" class="form-control" v-model="value" :placeholder ref="inputElement" :id :min :max :required>
  </div>
</template>

<script setup lang="ts">
import { useFocus } from '@vueuse/core'

const {
  focus,
  id = Math.random().toString(36).substring(2, 12)
} = defineProps<{
  placeholder?: string,
  focus?: boolean,
  id?: string,
  min?: number | string,
  max?: number | string,
  required?: boolean
}>()

const value = defineModel<number>()
const inputElement = ref()

if (focus) {
  useFocus(inputElement, { initialValue: true })
}
</script>