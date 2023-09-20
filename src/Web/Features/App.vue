<template>
  <div class="container-fluid vh-100">
    <Navbar />
    <Breadcrumb />
    <RouterView v-slot="{ Component }">
      <template v-if="Component">
        <suspense timeout="30">
          <template #default>
            <component :is="Component"></component>
          </template>
          <template #fallback>
            Loading...
          </template>
        </suspense>
      </template>
    </RouterView>
  </div>
</template>