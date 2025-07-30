<template>
  <div>
    <ul class="nav nav-tabs">
      <tab-header v-for="tab in tabs" v-bind="tab"></tab-header>
    </ul>
    <slot></slot>
  </div>
</template>

<script setup lang="ts">
import { onBeforeUnmount, onMounted, provide } from 'vue'
import { TabDataWithId, TabProvider } from './tabs'

const tabs = defineModel<TabDataWithId[]>('tabs', {
  default: []
})
const active = defineModel<TabDataWithId>('active')

const setActive = (id: string) => {
  active.value = tabs.value.find(t => t.id === id)
}
const setDefaultTab = () => {
  if (!active.value && tabs.value.length > 0) {
    setActive(tabs.value[0].id)
  }
}

const add = (tab: TabDataWithId) => {
  tabs.value = [...tabs.value, tab]

  setDefaultTab()
}
const remove = (id: string) => {
  tabs.value = tabs.value.filter(t => t.id !== id)

  if (active.value?.id === id) {
    setDefaultTab()
  }
}
const update = async (id: string, props: Record<string, any>) => {
  const tab = tabs.value.find(t => t.id === id)

  Object.assign(tab, props)

  tabs.value = [...tabs.value]
}

provide<TabProvider>("TabProvider", {
  tabs,
  active,
  add,
  remove,
  update,
  setActive,
})

onMounted(() => {
  setDefaultTab()
})

onBeforeUnmount(() => {
  tabs.value = []
  active.value = null
})
</script>