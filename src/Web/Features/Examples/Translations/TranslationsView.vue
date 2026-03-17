<template>
  <div class="container py-4">
    <div class="row">
      <div class="col-lg-8 offset-lg-2">
        <div class="d-flex justify-content-between align-items-center mb-4">
          <h1>{{ $t.navigation.translations() }}</h1>
          <div class="d-flex gap-2">
            <btn v-for="locale in localization.availableLocales" :key="locale" :variant="localization.locale.value === locale ? 'primary' : 'outline-primary'" @click="localization.setLocale(locale)">
              {{ localization.getLocaleName(locale) }}
            </btn>
          </div>
        </div>

        <div class="card mb-3">
          <div class="card-body">
            <h2>{{ $t.welcome() }}</h2>
            <p class="text-muted">Current language: <strong>{{ localization.getLocaleName(localization.locale.value) }}</strong></p>
          </div>
        </div>

        <div class="card mb-3">
          <div class="card-body">
            <h5 class="card-title">Common Translations</h5>
            <div class="d-flex flex-wrap gap-2">
              <btn variant="secondary">{{ $t.save() }}</btn>
              <btn variant="secondary">{{ $t.cancel() }}</btn>
              <btn variant="secondary">{{ $t.edit() }}</btn>
              <btn variant="secondary">{{ $t.delete() }}</btn>
            </div>
          </div>
        </div>

        <div class="card">
          <div class="card-body">
            <h5 class="card-title">Programmatic Usage (Type-Safe)</h5>
            <p class="text-muted mb-3">Using computed properties with full TypeScript autocomplete:</p>
            <ul class="list-unstyled">
              <li v-for="item in menuItems" :key="item.path" class="mb-2">
                <code class="text-muted">{{ item.path }}</code> → <strong>{{ item.label }}</strong>
              </li>
            </ul>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed, inject } from 'vue'
import { LocalizationService } from '../../Infrastructure/Localization/LocalizationService'

const localization = inject(LocalizationService.token)!

// Computed property with type-safe translation access
const menuItems = computed(() => [
  { path: '/home', label: localization.t.navigation.home() },
  { path: '/examples', label: localization.t.navigation.examples() },
  { path: '/translations', label: localization.t.navigation.translations() },
])

definePage({
  meta: {
    title: 'Translations',
  }
})
</script>
