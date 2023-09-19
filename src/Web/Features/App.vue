<template>
  <div class="container-fluid vh-100">
    <Navbar />
    <Breadcrumb />
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
  import { onErrorCaptured, ref } from 'vue'
  import { Router } from './router'
  import { showErrorDialog } from '../Util/Client/client-error-handler';

  const error = ref(null)

  onErrorCaptured(e => {
    error.value = e

    return showErrorDialog(e)
  })

  Router.onError(e => {
    error.value = e
    showErrorDialog(e)
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