<template>
  <li :key="id" @click="tabProvider.setActive(id)" class="nav-item">
    <span class="nav-link" :class="{ 'active': isActive }">
      {{ header }}
    </span>
  </li>
</template>

<script setup lang="ts">
import { inject } from 'vue'
import { TabProvider } from './tabs'

const { header, id } = defineProps<{ header: string; id: string }>()

const tabProvider = inject<TabProvider>("TabProvider")!

const isActive = computed(() => tabProvider.active.value?.id === id)
</script>

<style scoped>
.nav-link {
  cursor: pointer;
}
</style>