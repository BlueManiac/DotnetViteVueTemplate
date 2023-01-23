<template>
  <Navbar />
  <div class="container-fluid vh-100">
    <RouterView v-slot="{ Component }">
      <template v-if="Component">
          <suspense timeout="30">
              <template #default>
                  <problem-details v-if="error" :error="error" :component="Component" />
                  <component v-else :is="Component"></component>
              </template>
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
    console.clear()
  })
</script>