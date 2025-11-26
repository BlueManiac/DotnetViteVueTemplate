<template>
  <div>
    <label v-if="slots.default" class="form-label" :for="id">
      <slot />
    </label>
    <input :type class="form-control" v-model="value" :placeholder ref="inputElement" :id :required>
  </div>
</template>

<script setup lang="ts">
import { useFocus } from '@vueuse/core'
import { useSlots } from 'vue'

const {
  placeholder,
  focus,
  type = 'text',
  id = Math.random().toString(36).substring(2, 12),
  required
} = defineProps<{
  placeholder?: string,
  focus?: boolean,
  type?: string,
  id?: string,
  required?: boolean
}>()

const value = defineModel<unknown>()
const inputElement = ref()
const slots = useSlots()

if (focus) {
  useFocus(inputElement, { initialValue: true })
}
</script>