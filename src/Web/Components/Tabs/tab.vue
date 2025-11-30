<template>
  <slot v-if="tabProvider.active.value?.id === id"></slot>
</template>

<script setup lang="ts">
import { inject, onBeforeMount, onBeforeUnmount, watch } from 'vue'
import { TabProvider } from './tabs'

const { header, id = Math.random().toString(36).substring(2, 12) } = defineProps<{
  header?: string,
  id?: string
}>()

const tabProvider = inject<TabProvider>("TabProvider")!

watch(() => header, () => {
  if (header !== undefined) {
    tabProvider.update(id, { header })
  }
})

onBeforeMount(() => {
  if (header !== undefined) {
    tabProvider.add({ header, id })
  }
})

onBeforeUnmount(() => {
  tabProvider.remove(id)
})

</script>