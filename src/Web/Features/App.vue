<template>
  <Navbar />
  <div class="container-fluid vh-100 d-flex justify-content-center">
    <div class="col-4 text-center mt-5">
      <RouterView v-slot="{ Component }">
        <template v-if="Component">
          <suspense>
            <component :is="Component"></component>
            <template #fallback>
              Loadings...
            </template>
          </suspense>
        </template>
      </RouterView>
    </div>
  </div>
</template>