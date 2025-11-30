<template>
  <nav class="navbar navbar-expand-md">
    <router-link class="navbar-brand" to="/">
      {{ config.applicationName }}
    </router-link>
    <button class="navbar-toggler" type="button" data-bs-toggle="collapse" data-bs-target="#navbarSupportedContent" aria-controls="navbarSupportedContent" aria-expanded="false" aria-label="Toggle navigation">
      <span class="navbar-toggler-icon"></span>
    </button>
    <div class="collapse navbar-collapse" id="navbarSupportedContent">
      <ul class="navbar-nav nav-underline me-auto mb-2 mb-md-0">
        <template v-for="route in navigationRoutes">
          <li v-if="route.children && route.children.find(x => x.meta?.title)" class="nav-item dropdown">
            <router-link v-if="route.meta?.fullPath" class="nav-link dropdown-toggle" :to="route.meta.fullPath" :id="route.meta.id" role="button" data-bs-toggle="dropdown" aria-expanded="false">
              {{ route.meta.title }}
            </router-link>
            <span v-else class="nav-link dropdown-toggle" :id="route.meta?.id" role="button" data-bs-toggle="dropdown" aria-expanded="false">
              {{ route.meta?.title }}
            </span>
            <ul class="dropdown-menu mt-0" aria-labelledby="navbarDropdown">
              <template v-for="item in route.children">
                <li v-if="item.meta?.title && item.meta.fullPath"><router-link class="dropdown-item" :to="item.meta.fullPath">{{ item.meta.title }}</router-link></li>
              </template>
            </ul>
          </li>
          <li v-else-if="route.meta?.fullPath" class="nav-item">
            <router-link class="nav-link" aria-current="page" :to="route.meta.fullPath">{{ route.meta.title }}</router-link>
          </li>
        </template>
      </ul>
      <div class="navbar-nav float-end">
        <div v-if="profile.isLoggedIn.value" class="nav-link">{{ profile.userName }}</div>
        <div class="nav-item nav-link">
          <router-link v-if="profile.isLoggedIn.value" to="/auth/login" @click="logout()">Logout</router-link>
          <router-link v-else to="/auth/login">Login</router-link>
        </div>
        <color-theme-toggle />
      </div>
    </div>
  </nav>
</template>

<script setup lang="ts">
import { inject } from 'vue'
import { AppConfig } from '../AppConfig'
import { AuthService } from '../Auth/AuthService'
import { Profile } from '../Auth/Profile'
import { navigationRoutes } from '../router'

const config = inject(AppConfig)
const authService = inject(AuthService)
const profile = inject(Profile)

const logout = () => authService.logout()
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