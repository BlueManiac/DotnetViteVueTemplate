<template>
  <li class="nav-item dropdown">
    <button class="btn btn-link nav-link dropdown-toggle d-flex align-items-center" id="bd-theme" type="button" data-bs-toggle="dropdown" data-bs-display="static">
      <component :is="currentTheme.icon" class="my-1" />
    </button>
    <ul class="dropdown-menu dropdown-menu-end" aria-labelledby="bd-theme-text" data-bs-popper="static">
      <li v-for="theme in themes">
        <button @click="setTheme(theme.id)" type="button" class="dropdown-item d-flex align-items-center" v-bind:class="{ 'active': theme.id == currentTheme.id }" :data-bs-theme-value="theme.id" aria-pressed="false">
          <component :is="theme.icon" class="me-1" />
          {{theme.name}}
          <IconMdiCheck class="ms-auto" v-if="theme.id == currentTheme.id" />
        </button>
      </li>
    </ul>
  </li>
</template>

<script setup>
  import { themes, currentTheme, setTheme } from './color-schemes'
</script>