<template>
  <div>
    <label v-if="slots.default" class="form-label">
      <slot />
    </label>
    <input :type class="form-control" v-model="value" :placeholder ref="target">
  </div>
</template>

<script setup lang="ts">
import { useFocus } from '@vueuse/core'
import { useSlots } from 'vue'

const { focus, type = 'text' } = defineProps<{ placeholder?: string, focus?: boolean, type?: string }>()

const value = defineModel<any>()
const slots = useSlots()
const target = ref()

if (focus) {
  useFocus(target, { initialValue: true })
}
</script>