# GitHub Copilot Instructions - DotnetViteVueTemplate

This project is a full-stack ASP.NET Core + Vue 3 template with custom patterns and conventions.

## Documentation

For comprehensive information, refer to:
- **[Project README](../README.md)** - Overview, quick start, technology stack

## Quick Reference

### Technology Stack
- Backend: .NET 10, ASP.NET Core, SignalR
- Frontend: Vue 3 (Composition API only), Vite 7, TypeScript
- UI: Bootstrap 5, custom form components
- Build: pnpm workspace, unplugin ecosystem

### Project Structure

```
src/Web/
├── Features/              # Vertical slice modules
│   ├── {Feature}/
│   │   ├── {Feature}Module.cs      # Backend: IModule implementation
│   │   ├── {Feature}Service.ts     # Frontend: Injectable service
│   │   └── Pages/                  # Auto-registered routes
├── Components/            # Reusable UI components (auto-imported)
├── Util/
│   ├── Client/           # Frontend utilities (di.ts, fetch.ts, signalr.ts)
│   └── Modules/          # Backend module system (IModule.cs)
```

### Core Patterns

#### 1. Custom Dependency Injection
**DO** use class-based injection instead of string keys:
```typescript
// ✅ Correct
const api = inject(ApiService)!
const profile = inject(Profile)!

// ❌ Wrong
const api = inject('apiService')
```

**File**: [`src/Web/Util/Client/di.ts`](../src/Web/Util/Client/di.ts)
**DO** implement `IModule` for all features:
```csharp
public class MyFeatureModule : IModule
{
    public static void AddServices(WebApplicationBuilder builder)
    {
        builder.Services.AddScoped<MyService>();
    }

    public static void MapRoutes(WebApplication app)
    {
        var group = app.MapGroup("/api/myfeature");
        group.MapGet("/data", async (MyService svc) => Results.Ok(await svc.GetData()));
    }
}
```

**File**: [`src/Web/Util/Modules/IModule.cs`](../src/Web/Util/Modules/IModule.cs)

#### 3. Component Conventions
**DO** use kebab-case for component files and elements:
```vue
<!-- ✅ Correct -->
<input-text v-model="name" required>Name</input-text>
<btn @click="save">Save</btn>

<!-- ❌ Wrong -->
<InputText v-model="name" />
<Button @click="save" />
```

**DO** use Composition API with `<script setup lang="ts">`:
```vue
<script setup lang="ts">
const { required = false } = defineProps<{
  required?: boolean
}>()

const value = defineModel<string>()
</script>
```

**Location**: [`src/Web/Components/`](../src/Web/Components/)

#### 4. File-Based Routing
Routes are auto-generated from:
- `Features/*/Pages/*.vue` → Flattened routes (e.g., `/auth/login`)
- Manual routes in `Features/*/routes.ts`

**DO** use `definePage` for route metadata:
```typescript
definePage({
  meta: {
    auth: false,
    title: 'Login',
    center: true
  }
})
```

**Files**: [`src/Web/Features/router.ts`](../src/Web/Features/router.ts), [`src/Web/vite.config.ts`](../src/Web/vite.config.ts)

#### 5. API Service Pattern
**DO** use `ApiService` for all HTTP calls:
```typescript
const api = inject(ApiService)!

// Type-safe responses
const users = await api.get<User[]>('/api/users')
await api.post('/api/users', { name: 'John' })

// With loading state
const loading = ref(false)
const data = await api.get('/api/data', { loading })
```

**File**: [`src/Web/Features/ApiService.ts`](../src/Web/Features/ApiService.ts)

#### 6. SignalR Integration
**DO** use `api.useSignalr` with typed sender/receiver:
```typescript
const api = inject(ApiService)!

type Sender = {
  SendMessage(msg: string): Promise<void>
}

type Receiver = {
  messageReceived: Ref<string>
}

const { sender, receiver } = api.useSignalr<Sender, Receiver>('/api/hub')

await sender.SendMessage('Hello')
watch(receiver.messageReceived, (msg) => console.log(msg))
```

**File**: [`src/Web/Util/Client/signalr.ts`](../src/Web/Util/Client/signalr.ts)

#### 7. Modal System
**DO** use `showModal` for imperative modals:
```typescript
import { showModal } from '@/Components/Modals/modal'

const result = await showModal(ConfirmModal, {
  title: 'Confirm',
  message: 'Are you sure?'
}, { confirmed: false })

if (result?.confirmed) {
  // User confirmed
}
```

**Files**: [`src/Web/Components/Modals/`](../src/Web/Components/Modals/)

#### 8. Notifications
**DO** use `NotificationService` for displaying notifications:
```typescript
const notifications = inject(NotificationService)!

notifications.notify({ type: 'success', title: 'Saved' })

// Manual error handling (optional - errors bubble up to main.ts by default)
try {
  await api.post('/endpoint', data)
} catch (error) {
  notifications.notifyError(error)
}
```

**Note**: Uncaught errors automatically bubble up to `onErrorCaptured` in `App.vue` and are handled globally via `NotificationService`, so try/catch is not required everywhere.

**File**: [`src/Web/Components/Notifications/notifications.ts`](../src/Web/Components/Notifications/notifications.ts)

#### 9. Form Validation
**DO** use `v-validate` directive on forms:
```vue
<form v-validate @submit="onSubmit">
  <input-text v-model="name" required>Name</input-text>
  <input-text v-model="email" type="email" required>Email</input-text>
  <btn type="submit">Submit</btn>
</form>
```

**File**: [`src/Web/Components/Validation/v-validate.ts`](../src/Web/Components/Validation/v-validate.ts)

#### 10. Data Tables
**DO** use `data-table` for large datasets:
```vue
<data-table 
  :items="users" 
  :fields="['name', 'email', 'role']"
  selectable
>
  <template #cell-email="{ item }">
    <a :href="`mailto:${item.email}`">{{ item.email }}</a>
  </template>
</data-table>
```

**Files**: [`src/Web/Components/Tables/`](../src/Web/Components/Tables/)

### Naming Conventions

| Type | Convention | Example |
|------|-----------|---------|
| Vue Components | kebab-case | `input-text.vue`, `data-table.vue` |
| TypeScript Classes | PascalCase | `ApiService`, `AuthService` |
| TypeScript Files | Match class name | `ApiService.ts`, `Profile.ts` |
| C# Modules | `{Feature}Module.cs` | `AuthModule.cs` |
| Routes | lowercase | `/auth/login`, `/examples/tables` |

### Code Style

#### TypeScript
- **Always** use Composition API (`<script setup>`)
- **Never** use Options API
- **Always** type props, emits, and refs
- **Prefer** `ref` and `computed` over `reactive`

#### Components
- **Use** Bootstrap 5 classes for styling
- **Wrap** native inputs with custom components (see [`Components/`](../src/Web/Components/))
- **Emit** events for parent communication
- **Use** slots for flexible content
- **Use** scoped CSS (`<style scoped>`) for component-specific styles
- **Use** CSS nesting for cleaner, more maintainable styles

#### Backend
- **Group** routes by feature using `MapGroup`
- **Use** minimal APIs with route handlers
- **Return** Problem Details for errors
- **Keep** modules self-contained

#### Comments
- **Only** add comments for non-obvious code - complex logic, workarounds, or business rules
- **Don't** comment self-explanatory code - well-named variables/functions are preferred
- **Do** document "why" over "what" - explain intent and reasoning, not mechanics
- **Use** XML doc comments (`///`) for public APIs in C#
- **Use** JSDoc comments (`/** */`) for exported TypeScript functions/classes when needed

### Auto-Imports

These are globally available (no imports needed):
- **Vue APIs**: `ref`, `computed`, `watch`, `watchEffect`, `onMounted`, etc.
- **Components**: All from `Components/` and `Features/*/Components/`
- **Icons**: `<IconCarbonHome />`, `<IconMdiAccount />`, etc.

Configured in: [`src/Web/vite.config.ts`](../src/Web/vite.config.ts)

### Common Pitfalls

❌ **Don't** use Options API:
```vue
<!-- Wrong -->
<script>
export default {
  data() { return { count: 0 } }
}
</script>
```

✅ **Do** use Composition API:
```vue
<script setup lang="ts">
const count = ref(0)
</script>
```

❌ **Don't** import from 'vue-original':
```typescript
// Wrong - vue-original is only for internal DI system implementation
import { provide } from 'vue-original'
```

✅ **Do** import from 'vue':
```typescript
// Correct - always use the custom DI-enabled 'vue' import
import { provide } from 'vue'
const api = inject(ApiService)!
```

❌ **Don't** create global state stores:
```typescript
// Wrong
export const globalState = reactive({ ... })
```

✅ **Do** use dependency injection:
```typescript
// Correct
export class StateService {
  state = reactive({ ... })
}
app.provide(StateService)
```

### Examples

Working examples are in [`src/Web/Features/Examples/`](../src/Web/Features/Examples/):
- **Components**: Form components showcase
- **Icons**: Icon usage patterns
- **Modals**: Modal system examples
- **Tables**: Data table with virtual scrolling
- **Tabs**: Tab system examples
- **Real-Time**: SignalR chat implementation
- **Errors**: Error handling patterns

Reference these when implementing new features.

### Resources

- [Vue 3 Docs](https://vuejs.org/) - Official Vue documentation
- [Vite Docs](https://vitejs.dev/) - Vite build tool
- [ASP.NET Core Docs](https://docs.microsoft.com/aspnet/core/) - Backend framework
- [Bootstrap 5 Docs](https://getbootstrap.com/docs/5.3/) - UI framework
- [VueUse](https://vueuse.org/) - Composition utilities
