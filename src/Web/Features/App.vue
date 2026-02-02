<template>
  <main :class="{ centered: isCentered, 'full': isFull }" class="container-fluid vh-100">
    <Navbar v-if="!isCentered && !isFull" class="page-navbar" />
    <Breadcrumb v-if="!isCentered && !isFull" class="page-breadcrumb" />
    <RouterView v-slot="{ Component }">
      <template v-if="Component">
        <suspense timeout="30">
          <template #default>
            <div class="page-main">
              <component :is="Component" />
            </div>
          </template>
          <template #fallback>
            <div class="page-main">
              Loading...
            </div>
          </template>
        </suspense>
      </template>
    </RouterView>
    <notifications />
    <modal-target />
  </main>
</template>

<script setup lang="ts">
import { inject, onErrorCaptured } from 'vue'
import { useRoute } from 'vue-router'
import { NotificationService } from '../Components/Notifications/notifications'
import { HttpError } from '/Util/Client/fetch'

const route = useRoute()
const notificationService = inject(NotificationService)

const isCentered = computed(() => route.meta?.display === 'centered')
const isFull = computed(() => route.meta?.display === 'full')

onErrorCaptured((error) => {
  console.error(error)

  if (error instanceof HttpError) {
    notificationService.notifyError(error, error, error.problemDetails)

    return false
  }

  // Handle error server side
  if (import.meta.hot && error instanceof Error) {
    import.meta.hot.send("vite-runtime-error-plugin:error", {
      message: error.message,
      stack: error.stack,
    })
  }

  // stop error from propagating so hmr doesn't break for async components
  return false
})
</script>

<style scoped>
main {
  display: grid;
  grid-template-areas:
    "navbar"
    "breadcrumb"
    "main";
  grid-auto-rows: 0fr;
  grid-template-columns: 1fr;

  .page-navbar {
    grid-area: navbar;
  }

  .page-breadcrumb {
    grid-area: breadcrumb;
  }

  .page-main {
    grid-area: main;
  }

  &.centered {
    grid-template-areas: "main";
    grid-template-rows: 1fr;
    grid-template-columns: 1fr;

    .page-main {
      display: flex;
      justify-content: center;
      align-items: center;
    }

    .page-navbar,
    .page-breadcrumb {
      display: none;
    }
  }

  &.full {
    grid-template-areas: "main";
    grid-template-rows: 1fr;
    grid-template-columns: 1fr;

    .page-main {
      display: flex;
      width: 100%;
      height: 100%;

      > * {
        width: 100%;
        height: 100%;
      }
    }

    .page-navbar,
    .page-breadcrumb {
      display: none;
    }
  }
}
</style>