import type { InjectionKey } from 'vue'

export class AppConfig {
  static readonly token: InjectionKey<AppConfig> = Symbol(AppConfig.name)

  apiUrl = import.meta.env.VITE_BACKEND_API_URL
  applicationName = import.meta.env.VITE_APPLICATION_NAME
}
