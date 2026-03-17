# Translations

Type-safe internationalization system with reactive locale switching. Language preferences are automatically saved and restored.

**Example page**: `/examples/translations`

## Usage

### Simple Translations

All translation keys are called as functions with full TypeScript autocomplete:

```vue
<template>
  <!-- Simple string translations -->
  <h1>{{ $t.welcome() }}</h1>
  <btn>{{ $t.save() }}</btn>
  
  <!-- Nested translations -->
  <p>{{ $t.navigation.home() }}</p>
  <p>{{ $t.navigation.examples() }}</p>
</template>
```

The `$t` function is globally available - no imports needed.

### Parameterized Translations

For dynamic content, define functions in your translation files:

```typescript
// Localization/en.ts
const en = {
  greeting: ({ name }: { name: string }) => `Hello, ${name}!`,
  itemCount: ({ count }: { count: number }) => `You have ${count} item${count !== 1 ? 's' : ''}`
}
```

```vue
<template>
  <p>{{ $t.greeting({ name: userName }) }}</p>
  <p>{{ $t.itemCount({ count: items.length }) }}</p>
</template>
```

### Using in Script

Access translations programmatically via the service:

```vue
<script setup lang="ts">
const localization = inject(LocalizationService.token)!

// Get translation
const message = localization.t.welcome()

// Use in composables, watchers, etc.
watch(state, () => {
  notifications.notify({ title: localization.t.success() })
})
</script>
```

### Programmatic String-Based Lookup

For dynamic keys or when the key is stored in a variable:

```vue
<script setup lang="ts">
const localization = inject(LocalizationService.token)!

// Dynamic key lookup
const key = computed(() => isAdmin.value ? 'admin.dashboard' : 'user.dashboard')
const title = computed(() => localization.translate(key.value))

// Useful in loops or computed values
const menuItems = ['home', 'profile', 'settings'].map(key => ({
  label: localization.translate(`navigation.${key}`),
  path: `/${key}`
}))
</script>
```

**Note:** Use `$t.key()` for type-safe autocomplete, `translate(key)` for dynamic keys.

### Switching Languages

```vue
<script setup lang="ts">
const localization = inject(LocalizationService.token)!

localization.setLocale('sv') // Switch to Swedish

// Or use buttons
<btn @click="localization.setLocale('en')">English</btn>
<btn @click="localization.setLocale('sv')">Svenska</btn>
</script>
```

Or use the `<language-switcher />` component for a dropdown UI with current language indicator.

## Adding Translations

### 1. Edit `Localization/en.ts` (base locale)

```typescript
const en = {
  localeName: 'English',
  myFeature: {
    title: 'My Feature',
    description: ({ name }: { name: string }) => `Welcome, ${name}!`
  }
}
```

Types are automatically inferred from `en.ts` - no manual updates needed.

### 2. Edit `Localization/sv.ts` (and other locales)

```typescript
const sv = {
  localeName: 'Svenska',
  myFeature: {
    title: 'Min Funktion',
    description: ({ name }: { name: string }) => `Välkommen, ${name}!`
  }
} satisfies Translation // Validates structure matches en.ts
```

## Adding a New Language

1. Create `Localization/de.ts`:
   ```typescript
   const de = {
     localeName: 'Deutsch',
     // ... copy structure from en.ts
   } satisfies Translation
   ```

2. Register in `LocalizationService.ts`:
   ```typescript
   import de from '../../Localization/de'
   export const dictionaries = { en, sv, de } as const
   ```

## Features

- ✅ Full TypeScript autocomplete
- ✅ Reactive language switching (no page reload)
- ✅ Persistent language preference (localStorage)
- ✅ Browser language detection
- ✅ Parameterized translations
- ✅ Error handling with fallbacks

## Error Handling

Missing translations are handled gracefully:
- **Missing keys**: Returns `[key.path]` and logs warning to console
- **Invalid locale**: Falls back to English ('en')
- **Function errors**: Catches exceptions and returns `[key.path]`

In development, check the console for translation warnings.
