# DotnetViteVueTemplate

A modern full-stack web application template combining ASP.NET Core with Vue 3 and Vite. This template provides a production-ready foundation with authentication, real-time communication, custom dependency injection, and a comprehensive component library.

> **Note**: This is an opinionated template that tightly integrates server-side C# (ASP.NET Core) with client-side Vue 3 in a single project. While this monolithic approach is less common than separate frontend/backend repositories, it offers simplified deployment, shared type safety opportunities, and streamlined development for small-to-medium applications.

## Features

- **Modern Stack**: .NET 9, Vue 3 (Composition API), Vite 7, TypeScript
- **Custom DI System**: Type-safe dependency injection wrapping Vue's provide/inject
- **Vertical Slice Architecture**: Self-contained feature modules with IModule pattern
- **File-Based Routing**: Auto-registered routes with type safety via unplugin-vue-router
- **Component Library**: Bootstrap 5-based components with auto-import
- **Real-Time Communication**: SignalR integration with typed proxies
- **Authentication**: Bearer token authentication with automatic refresh
- **Developer Experience**: HMR, HTTPS dev server, Vue Inspector, runtime error overlay

## Quick Start

### Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [Node.js](https://nodejs.org/)
- [pnpm](https://pnpm.io/) - Install via `npm install -g pnpm`

### Running the Application

**Recommended: Use VS Code Tasks**

Press `Ctrl+Shift+P` (or `Cmd+Shift+P` on Mac) and run **Tasks: Run Task**, then choose:

- **watch all** - Starts both ASP.NET and Vite dev servers
- **watch backend** - ASP.NET Core with hot reload only
- **watch frontend** - Vite dev server only
- **build** - Build the project
- **publish** - Build and publish for production
- **update** - Update both .NET and npm dependencies
- **update backend** - Update .NET dependencies only
- **update frontend** - Update npm dependencies only

**Manual Commands**

1. **Clone the repository**
   ```powershell
   git clone https://github.com/BlueManiac/DotnetViteVueTemplate.git
   cd DotnetViteVueTemplate
   ```

2. **Install dependencies**
   ```powershell
   pnpm install
   ```

3. **Trust the development certificate** (first-time setup)
   ```powershell
   dotnet dev-certs https --trust
   ```

4. **Run the application**
   
   Start the backend (ASP.NET Core):
   ```powershell
   cd src/Web
   dotnet run
   ```
   
   In a separate terminal, start the frontend (Vite):
   ```powershell
   cd src/Web
   pnpm run dev
   ```

5. **Open in browser**
   - Navigate to [https://localhost:7126](https://localhost:7126)

The application will start with hot module replacement enabled. Changes to Vue files will update instantly, and ASP.NET Core changes will trigger a browser refresh.

## Technology Stack

### Backend
- **[.NET 9](https://dotnet.microsoft.com/download/dotnet/9.0)** - Modern web framework
- **[SignalR](https://docs.microsoft.com/aspnet/core/signalr/)** - Real-time communication
- **[ASP.NET Core Identity](https://docs.microsoft.com/aspnet/core/security/authentication/identity)** - Authentication

### Frontend
- **[Vue 3](https://vuejs.org/)** - Progressive JavaScript framework (Composition API)
- **[Vite 7](https://vitejs.dev/)** - Next-generation build tool
- **[TypeScript](https://www.typescriptlang.org/)** - Type-safe JavaScript
- **[Vue Router](https://router.vuejs.org/)** - Official routing library
- **[Bootstrap 5](https://getbootstrap.com/)** - UI component framework
- **[@vueuse/core](https://vueuse.org/)** - Composition utilities

### Build Tools & Plugins
- **[unplugin-vue-router](https://github.com/posva/unplugin-vue-router)** - File-based routing with type generation
- **[unplugin-vue-components](https://github.com/antfu/unplugin-vue-components)** - Auto-import components
- **[unplugin-auto-import](https://github.com/antfu/unplugin-auto-import)** - Auto-import Vue APIs
- **[unplugin-icons](https://github.com/antfu/unplugin-icons)** - Icon components from Iconify

## Project Structure

```
DotnetViteVueTemplate/
├── src/
│   └── Web/                          # Main web application
│       ├── Features/                 # Feature modules (vertical slices)
│       │   ├── Auth/                 # Authentication module
│       │   │   ├── AuthModule.cs     # Backend module registration
│       │   │   ├── AuthService.ts    # Frontend authentication service
│       │   │   └── Pages/            # Auth routes (auto-registered)
│       │   ├── Examples/             # Example feature implementations
│       │   └── Home/                 # Home page feature
│       ├── Components/               # Reusable UI components
│       │   ├── ColorThemes/          # Theme switcher
│       │   ├── Modals/               # Modal system
│       │   ├── Notifications/        # Notification system
│       │   ├── Tables/               # Data table with virtual scrolling
│       │   ├── Tabs/                 # Tab container system
│       │   └── Validation/           # Form validation directive
│       ├── Util/                     # Utilities
│       │   ├── Client/               # Frontend utilities
│       │   │   ├── di.ts             # Custom dependency injection
│       │   │   ├── fetch.ts          # HTTP utilities
│       │   │   └── signalr.ts        # SignalR integration
│       │   └── Modules/              # Backend module system
│       │       ├── IModule.cs        # Module interface
│       │       └── WebApplicationExtensions.cs
│       ├── Program.cs                # ASP.NET entry point
│       ├── vite.config.ts            # Vite configuration
│       └── package.json              # Frontend dependencies
├── .github/
│   └── copilot-instructions.md       # GitHub Copilot context
└── pnpm-workspace.yaml               # pnpm workspace configuration
```

## Development Guide

For detailed information about the architecture, patterns, and conventions used in this template, see:

📖 **[GitHub Copilot Instructions](.github/copilot-instructions.md)**

Topics covered:
- Custom Dependency Injection system
- Vertical Slice Architecture with IModule
- Component patterns and conventions
- File-based routing and typed routes
- API service patterns
- SignalR integration
- Modal, notification, and table systems
- Form validation
- Authentication flow
- Theme system

## Building for Production

**Recommended: Use VS Code Tasks**

Press `Ctrl+Shift+P` and run **Tasks: Run Task** → **publish**

**Manual Command**

```powershell
cd src/Web
dotnet publish -c Release
```

The build process will:
1. Install frontend dependencies via pnpm
2. Build frontend assets with Vite
3. Output static files to `wwwroot/`
4. Create a self-contained deployment package

## License

This template is provided as-is for use in your own projects.

## Resources

- [Vue 3 Documentation](https://vuejs.org/)
- [Vite Documentation](https://vitejs.dev/)
- [ASP.NET Core Documentation](https://docs.microsoft.com/aspnet/core/)
- [Bootstrap 5 Documentation](https://getbootstrap.com/docs/5.3/)
- [TypeScript Documentation](https://www.typescriptlang.org/docs/)
