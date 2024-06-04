<template>
  <main :class="{ centered: $route.meta?.centered }" class="container-fluid vh-100">
    <Navbar class="page-navbar" />
    <Breadcrumb class="page-breadcrumb" />
    <RouterView v-slot="{ Component }">
      <template v-if="Component">
        <suspense timeout="30">
          <template #default>
            <component :is="Component" class="page-main"></component>
          </template>
          <template #fallback>
            <div class="page-main">
              Loading...
            </div>
          </template>
        </suspense>
      </template>
    </RouterView>
  </main>
</template>

<style scoped>
main {
  display: grid;
  grid-template-areas:
    "navbar"
    "breadcrumb"
    "main";
  grid-template-rows: auto auto 1fr;
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
    place-items: center;

    .page-navbar,
    .page-breadcrumb {
      display: none;
    }
  }
}
</style>