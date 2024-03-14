<template>
  <nav aria-label="breadcrumb">
    <ol class="breadcrumb">
      <li class="breadcrumb-item" v-for="route in routePath">
        <router-link v-if="route.meta.fullPath" :to="route.meta.fullPath">{{ route.meta.title }}</router-link>
        <span v-else>{{ route.meta.title }}</span>
      </li>
    </ol>
  </nav>
</template>

<script setup>
import { routePath } from '../router';
</script>