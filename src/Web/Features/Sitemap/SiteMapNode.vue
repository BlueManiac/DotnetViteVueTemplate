<template>
  <span @click="expanded = !expanded">
    <router-link v-if="route.meta.fullPath" :to="route.name">{{ route.name }}</router-link>
    <template v-else>{{ route.path }}</template>
    - <template v-for="(value, key) in route.meta">
      <span class="text-warning" v-if="value">{{ key }}: {{ value }} - </span>
    </template>
    <span class="text-success">{{ route.component }}</span>
  </span>
  <pre v-if="expanded">{{ route }}</pre>
  <ul>
    <li v-for="route in route.children">
      <SiteMapNode :route />
    </li>
  </ul>
</template>

<script setup>
defineProps(['route'])

const expanded = ref(false)
</script>