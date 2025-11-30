<template>
  <span @click="expanded = !expanded">
    <router-link v-if="route.meta?.fullPath" :to="route.meta.fullPath">{{ route.name }}</router-link>
    <template v-else>{{ route.path }}</template>
    - <template v-for="(value, key) in route.meta">
      <span class="text-warning" v-if="value && key !== 'filePath'">{{ key }}: {{ value }} - </span>
    </template>
    <a class="text-success" @click.stop="openInEditor()">{{ filePath }}</a>
  </span>
  <pre v-if="expanded">{{ route }}</pre>
  <ul>
    <li v-for="item in route.children">
      <SiteMapNode :route="item" />
    </li>
  </ul>
</template>

<script setup lang="ts">
import { RouteRecordRaw } from 'vue-router'

const { route } = defineProps<{ route: RouteRecordRaw }>()

const expanded = ref(false)

const filePath = computed(() => {
  if (typeof route.component == 'object')
    return (<any>route?.component)?.__file

  return route.meta?.filePath
})

const openInEditor = async () => {
  const path = `/__open-in-editor?file=${encodeURIComponent(filePath.value)}`

  await fetch(new URL(path, import.meta.url))
}
</script>

<style scoped>
a {
  cursor: pointer;
}
</style>