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
- Build: npm workspace, unplugin ecosystem

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
const api = inject(ApiService)
const profile = inject(Profile)

// ❌ Wrong
const api = inject('apiService')
```

#### 2. Module System
**DO** implement `IModule` for all features:
```csharp
public class MyFeatureModule : IModule
{
    // Optional: Only implement if you need to register services
    public static void AddServices(WebApplicationBuilder builder)
    {
        builder.Services.AddScoped<MyService>();
    }

    // Optional: Only implement if you need to map routes
    public static void MapRoutes(WebApplication app)
    {
        var group = app.MapGroup("/api/myfeature");
        group.MapGet("/data", async (MyService svc) => Results.Ok(await svc.GetData()));
    }
}
```

**Modules are auto-discovered** - All classes implementing `IModule` are automatically registered via `builder.AddModules()` and `app.MapModules()` in `Program.cs`. No manual registration required.

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

#### 4. File-Based Routing
Routes are auto-generated from:
- `Features/*/Pages/*.vue` → Flattened routes (e.g., `/auth/login`)
- Manual routes in `Features/*/routes.ts`

**DO** use `definePage` for route metadata:
```typescript
definePage({
  meta: {
    auth: false,
    centered: true
  }
})
```

#### 5. API Service Pattern
**DO** use `ApiService` for all HTTP calls:
```typescript
const api = inject(ApiService)

// Type-safe responses
const users = await api.get<User[]>('/api/users')
await api.post('/api/users', { name: 'John' })

// With loading state
const loading = ref(false)
const data = await api.get('/api/data', { loading })
```

#### 6. SignalR Integration
**DO** use `api.useSignalr` with typed sender/receiver:
```typescript
const api = inject(ApiService)

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

#### 7. Modal System
**DO** use `showModal` for imperative modals:
```typescript
import { showModal } from '/Components/Modals/modal'

const result = await showModal(ConfirmModal, {
  title: 'Confirm',
  message: 'Are you sure?'
}, { confirmed: false })

if (result?.confirmed) {
  // User confirmed
}
```

#### 8. Notifications
**DO** use `NotificationService` for displaying notifications:
```typescript
const notifications = inject(NotificationService)

notifications.notify({ type: 'success', title: 'Saved' })

// Manual error handling (optional - errors bubble up to main.ts by default)
try {
  await api.post('/endpoint', data)
} catch (error) {
  notifications.notifyError(error)
}
```

**Note**: Uncaught errors automatically bubble up to `onErrorCaptured` in `App.vue` and are handled globally via `NotificationService`, so try/catch is not required everywhere.

#### 9. Form Validation
**DO** use `v-validate` directive on forms:
```vue
<form v-validate @submit="onSubmit">
  <input-text v-model="name" required>Name</input-text>
  <input-text v-model="email" type="email" required>Email</input-text>
  <btn type="submit">Submit</btn>
</form>
```

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
- **Prefer** [VueUse](https://vueuse.org/) composables for common functionality (e.g., `useStorage`, `useEventListener`, `useDebounce`)

#### Components
- **Order** component sections as: `<template>`, `<script>`, `<style>`
- **Use** Bootstrap 5 classes for styling
- **Wrap** native inputs with custom components
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
- **Vue APIs**: `ref`, `computed` (other Vue APIs like `watch`, `onMounted` must be imported)
- **Components**: All from `Components/` and `Features/*/` (excluding `Pages/`)
- **Icons**: Use icon names **without** the `Icon` prefix
  - ✅ Correct: `<CarbonHome />`, `<MdiAccount />`, `<CarbonCloudUpload />`
  - ❌ Wrong: `<IconCarbonHome />`, `<IconMdiAccount />`, `<IconCarbonCloudUpload />`
  - Icons are from [Iconify](https://icon-sets.iconify.design/) via unplugin-icons
  - Common sets: `Mdi*` (Material Design Icons), `Carbon*` (IBM Carbon)

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
const api = inject(ApiService)
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

Working examples in `src/Web/Features/Examples/` include components, icons, modals, tables, tabs, real-time (SignalR), and error handling patterns.

### Running Commands

**DO** use the VS Code tasks for all operations:
- **Build/Run**: Use `run_task` tool with exact task IDs from workspace context (e.g., `"process: build backend"`, `"shell: watch frontend"`)
- **Check output**: Use `get_task_output` tool to view running task output
- **Verify changes**: Check task outputs to see compilation results and errors

**Available tasks** (see workspace tasks in context for exact IDs):
- `watch` - Run both frontend and backend dev servers
- `watch frontend` (ID: `"shell: watch frontend"`) - Vite dev server only
  - Output shows: Vite HMR updates, compilation errors, dependency optimization, client-side runtime errors
- `watch backend` (ID: `"process: watch backend"`) - ASP.NET Core with hot reload
  - Output shows: Application startup info, API endpoint registrations, runtime errors
- `build backend` (ID: `"process: build backend"`) - Build the project
- `publish` (ID: `"process: publish"`) - Build and publish for production
- `update` / `update frontend` / `update backend` - Update dependencies

**Managing tasks**:
- If a task fails or is not running, you can start it using `run_task` tool
- Tasks cannot be programmatically stopped - they must be manually terminated through VS Code's UI
- When checking if a watch task is running, use `get_task_output` - if the output shows the server is listening/ready, it's running; if it shows termination messages, it's not running and can be restarted

**DO NOT** attempt to:
- Run `npm run dev` or similar dev server commands
- Start additional dev servers
- Use `run_in_terminal` tool except when absolutely necessary (e.g., installing packages with `npm install <package>`)

**Installing packages**:
- You CAN use `npm install <package>` to install new packages
- The running frontend watch task should automatically detect the changes and update

### Verifying Changes

When making changes to the project, you can verify them by checking the watch frontend/backend tasks using `get_task_output` tool.

**Important**: When checking task outputs, only check the last 50 lines to avoid excessive context usage. Task outputs can be very long, and the most recent information is usually sufficient to verify changes.

Use these outputs to confirm that:
- Changes are being detected and recompiled
- No compilation or runtime errors occurred
- The dev servers are still running properly

**Note**: Only check the relevant task output - if changes were made only to frontend files (Vue, TypeScript, CSS), check the frontend task; if changes were made only to backend files (C#), check the backend task.

### Resources

- [Vue 3 Docs](https://vuejs.org/) - Official Vue documentation
- [Vite Docs](https://vitejs.dev/) - Vite build tool
- [ASP.NET Core Docs](https://docs.microsoft.com/aspnet/core/) - Backend framework
- [Bootstrap 5 Docs](https://getbootstrap.com/docs/5.3/) - UI framework
- [VueUse](https://vueuse.org/) - Composition utilities
