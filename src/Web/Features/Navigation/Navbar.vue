<template>
  <nav class="navbar navbar-expand-md">
    <router-link class="navbar-brand" to="/">{{applicationName}}</router-link>
    <button class="navbar-toggler" type="button" data-bs-toggle="collapse" data-bs-target="#navbarSupportedContent" aria-controls="navbarSupportedContent" aria-expanded="false" aria-label="Toggle navigation">
      <span class="navbar-toggler-icon"></span>
    </button>
    <div class="collapse navbar-collapse" id="navbarSupportedContent">
      <ul class="navbar-nav me-auto mb-2 mb-md-0">
        <template v-for="route in routes">
          <li v-if="route.children" class="nav-item dropdown">
            <router-link v-if="route.meta.fullPath" class="nav-link dropdown-toggle" :to="route.meta.fullPath" :id="route.meta.id" role="button" data-bs-toggle="dropdown" aria-expanded="false">
              {{route.meta.title}}
            </router-link>
            <span v-else class="nav-link dropdown-toggle" :id="route.meta.id" role="button" data-bs-toggle="dropdown" aria-expanded="false">
              {{route.meta.title}}
            </span>
            <ul class="dropdown-menu mt-0" aria-labelledby="navbarDropdown">
              <li v-for="route in route.children"><router-link class="dropdown-item" :to="route.meta.fullPath">{{route.meta.title}}</router-link></li>
            </ul>
          </li>
          <li v-else class="nav-item">
            <router-link class="nav-link" aria-current="page" :to="route.meta.fullPath">{{route.meta.title}}</router-link>
          </li>
        </template>
      </ul>
    </div>
  </nav>
</template>

<script setup>
  import { applicationName } from "../info"
  import { routes } from "../router"
</script>