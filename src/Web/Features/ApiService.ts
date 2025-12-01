import { until } from '@vueuse/core'
import { inject } from 'vue'
import { AppConfig } from './AppConfig'
import { Profile } from './Auth/Profile'
import { HealthService } from './Health/HealthService'
import { useApi } from '/Util/Client/fetch'
import { signalr, SignalrReciever, SignalrSender, useSignalr } from '/Util/Client/signalr'

export class ApiService {
  private profile = inject(Profile)
  private config = inject(AppConfig)
  private health = inject(HealthService)

  private api = useApi({
    apiUrl: this.config.apiUrl,
    intercept: async request => {
      if (import.meta.env.DEV) {
        await until(this.health.backendReady).toBe(true)
      }

      const accessToken = this.profile.accessToken.value
      if (accessToken && !request.headers?.['Authorization']) {
        request.headers ??= {}
        request.headers['Authorization'] = `Bearer ${accessToken}`
      }

      return request
    }
  })

  get = this.api.get
  post = this.api.post
  put = this.api.put
  delete = this.api.delete

  signalr<TSender extends SignalrSender, TReciever extends SignalrReciever>(url: string) {
    return signalr<TSender, TReciever>(this.config.apiUrl + url, () => this.profile?.accessToken?.value)
  }

  useSignalr<TSender extends SignalrSender, TReciever extends SignalrReciever>(url: string) {
    return useSignalr<TSender, TReciever>(this.config.apiUrl + url, () => this.profile?.accessToken?.value)
  }
}