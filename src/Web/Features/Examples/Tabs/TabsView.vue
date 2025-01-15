<template>
  {{ activeTab }}
  <br>
  <br>

  <tabs v-model:active="activeTab">
    <tab header="test1">
      test1
    </tab>
    <tab header="test2">
      test2
    </tab>
    <tab v-for="{ header } in list" :header>
      {{ header }}
    </tab>
  </tabs>
</template>

<script setup lang="ts">
import { TabData, TabDataWithId } from '/Components/Tabs/tabs'

const list = ref<TabData[]>([
  { header: "one" },
  { header: "two" },
  { header: "three" }
])

const activeTab = ref<TabDataWithId>()
</script>