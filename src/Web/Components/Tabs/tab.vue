<template>
  <slot v-if="active?.id === id"></slot>
</template>

<script setup lang="ts">
import { inject, onBeforeMount, onBeforeUnmount, watch } from 'vue'
import { TabProvider } from './tabs'

const { header, id = Math.random().toString(36).substring(2, 12) } = defineProps<{
  header?: string,
  id?: string
}>()

const { active, add, remove, update } = inject<TabProvider>("TabProvider")

watch(() => header, () => {
  update(id, {
    header: header
  })
})

onBeforeMount(() => {
  add({ header, id })
})

onBeforeUnmount(() => {
  remove(id)
})

</script>