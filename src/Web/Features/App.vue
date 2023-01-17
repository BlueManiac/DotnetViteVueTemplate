<template>
  <Navbar />
  <div class="container-fluid vh-100 pt-3">
    <RouterView v-slot="{ Component }">
      <problem-details v-if="error" :error="error" :component="Component" />
      <template v-else-if="Component">
        <suspense>
          <component :is="Component"></component>
          <template #fallback>
            Loading...
          </template>
        </suspense>
      </template>
    </RouterView>
  </div>
</template>

<script setup>
  import { onErrorCaptured, ref } from "vue"
  import { Router } from "./router"

  const error = ref(null)

  onErrorCaptured(e => {
    error.value = e

    return false
  })

  Router.afterEach(() => {
    error.value = null
  })

  import.meta.hot.on('vite:afterUpdate', () => {
    error.value = null
  })
</script>