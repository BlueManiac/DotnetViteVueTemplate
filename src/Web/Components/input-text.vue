<template>
  <div :class="floating ? 'form-floating' : undefined">
    <label v-if="!floating && slots.default" class="form-label" :for="id">
      <slot />
    </label>
    <input :type class="form-control" v-model="value" v-bind="$attrs" :placeholder="floating ? ' ' : ($attrs.placeholder as string | undefined)" ref="inputElement" :id :required>
    <label v-if="floating && slots.default" :for="id">
      <slot />
    </label>
  </div>
</template>

<script setup lang="ts">
import { useFocus } from '@vueuse/core'
import { useSlots } from 'vue'

const {
  autofocus,
  floating,
  type = 'text',
  id = Math.random().toString(36).substring(2, 12),
  required
} = defineProps<{
  autofocus?: boolean,
  floating?: boolean,
  type?: string,
  id?: string,
  required?: boolean
}>()

const value = defineModel<unknown>()
const inputElement = ref<HTMLInputElement>()
const slots = useSlots()

if (autofocus) {
  useFocus(inputElement, { initialValue: true })
}

defineOptions({ inheritAttrs: false })
</script>