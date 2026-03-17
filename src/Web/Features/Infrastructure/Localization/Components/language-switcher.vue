<template>
  <div class="nav-item dropdown">
    <button class="nav-link dropdown-toggle" type="button" id="languageDropdown" data-bs-toggle="dropdown" aria-expanded="false">
      {{ localization.locale.value.toUpperCase() }}
    </button>
    <ul class="dropdown-menu dropdown-menu-end" aria-labelledby="languageDropdown">
      <li v-for="locale in localization.availableLocales" :key="locale">
        <button class="dropdown-item" :class="{ active: localization.locale.value === locale }" @click="localization.setLocale(locale)">
          {{ localization.getLocaleName(locale) }}
        </button>
      </li>
    </ul>
  </div>
</template>

<script setup lang="ts">
import { inject } from 'vue'
import { LocalizationService } from '../LocalizationService'

const localization = inject(LocalizationService.token)!
</script>