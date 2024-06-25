<template>
  <ul class="list-unstyled">
    <li v-for=" route in $router.options.routes">
      <SiteMapNode :route />
    </li>
  </ul>
</template>

<script setup lang="ts">
definePage({
  meta: {
    title: 'Sitemap'
  }
})
</script>