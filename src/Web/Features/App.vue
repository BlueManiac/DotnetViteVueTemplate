<template>
  <div class="container-fluid vh-100">
    <Navbar />
    <Breadcrumb />
    <problem-details v-if="error" :error="error" />
    <RouterView v-slot="{ Component }">
      <template v-if="Component">
        <suspense timeout="30">
          <template #default>
            <component v-if="!error" :is="Component"></component>
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

  Router.onError(e => {
    error.value = e
  })

  Router.afterEach(() => {
    error.value = null
  })

  import.meta.hot?.on('vite:beforeUpdate', () => {
    console.clear()
  })
  import.meta.hot?.on('vite:afterUpdate', () => {
    error.value = null
  })
</script>