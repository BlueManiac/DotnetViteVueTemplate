<template>
  <nav class="navbar navbar-expand-md">
    <router-link class="navbar-brand" to="/">
      {{ applicationName }}
    </router-link>
    <button class="navbar-toggler" type="button" data-bs-toggle="collapse" data-bs-target="#navbarSupportedContent" aria-controls="navbarSupportedContent" aria-expanded="false" aria-label="Toggle navigation">
      <span class="navbar-toggler-icon"></span>
    </button>
    <div class="collapse navbar-collapse" id="navbarSupportedContent">
      <ul class="navbar-nav nav-underline me-auto mb-2 mb-md-0">
        <template v-for="route in navigationRoutes">
          <li v-if="route.children && route.children.find(x => x.meta.title)" class="nav-item dropdown">
            <router-link v-if="route.meta.fullPath" class="nav-link dropdown-toggle" :to="route.meta.fullPath" :id="route.meta.id" role="button" data-bs-toggle="dropdown" aria-expanded="false">
              {{ route.meta.title }}
            </router-link>
            <span v-else class="nav-link dropdown-toggle" :id="route.meta.id" role="button" data-bs-toggle="dropdown" aria-expanded="false">
              {{ route.meta.title }}
            </span>
            <ul class="dropdown-menu mt-0" aria-labelledby="navbarDropdown">
              <template v-for="route in route.children">
                <li v-if="route.meta.title"><router-link class="dropdown-item" :to="route.meta.fullPath">{{ route.meta.title }}</router-link></li>
              </template>
            </ul>
          </li>
          <li v-else class="nav-item">
            <router-link class="nav-link" aria-current="page" :to="route.meta.fullPath">{{ route.meta.title }}</router-link>
          </li>
        </template>
      </ul>
      <div class="navbar-nav float-end">
        <div class="nav-link">{{ userName }}</div>
        <div class="nav-item nav-link">
          <router-link v-if="isLoggedIn" to="/auth/login" @click="logout()">Logout</router-link>
          <router-link v-else to="/auth/login">Login</router-link>
        </div>
        <color-theme-toggle />
      </div>
    </div>
  </nav>
</template>

<script setup>
import { isLoggedIn, logout } from '../Auth/AuthenticationService'
import { applicationName } from '../info'
import { navigationRoutes } from '../router'
import { userName } from '/Features/Auth/Profile'
</script>

<style scoped>
.nav-underline {
  --bs-nav-underline-gap: 0.5rem;
}

.nav-underline .nav-link.active,
.nav-underline .show > .nav-link {
  font-weight: normal;
}
</style>