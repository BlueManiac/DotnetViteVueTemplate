<template>
  <ul class="list-unstyled">
    <li v-for=" route in $router.options.routes">
      <SiteMapNode :route />
    </li>
  </ul>
</template>

<script setup lang="ts">
import { definePage } from 'vue-router/auto'

definePage({
  meta: {
    title: 'Sitemap'
  }
})
</script>