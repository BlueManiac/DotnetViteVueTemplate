<template>
  <modal v-model="modelValue">
    <template #header>
      {{ header }}
    </template>
    <p class="m-0">
      Modal state: {{ modelValue }}
    </p>
    <template #footer>
      <btn @click="set(true)">Yes</btn>
      <btn @click="set(false)">No</btn>
    </template>
  </modal>
</template>

<script setup lang="ts">
import { ModalState } from '/Components/Modals/modal'

const { header } = defineProps<{
  header: string
}>()

const modelValue = defineModel<ModalState<boolean>>({ default: { visible: false } })

const set = (result: boolean) => {
  modelValue.value.state = result
  modelValue.value.close()
}
</script>