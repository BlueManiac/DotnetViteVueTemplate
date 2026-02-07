import { until } from '@vueuse/core'
import { AppConfig } from './AppConfig'
import { Profile } from './Auth/Profile'
import { TokenValidator } from './Auth/TokenValidator'
import { HealthService } from './Health/HealthService'
import { useApi } from '/Util/Client/fetch'
import { signalr, SignalrReciever, SignalrSender, useSignalr } from '/Util/Client/signalr'

export class ApiService {
  private config: AppConfig
  private profile: Profile
  private health: HealthService
  private tokenValidator: TokenValidator
  private api: ReturnType<typeof useApi>

  constructor(
    tokenValidator: TokenValidator,
    config: AppConfig,
    profile: Profile,
    health: HealthService
  ) {
    this.config = config
    this.profile = profile
    this.health = health
    this.tokenValidator = tokenValidator

    this.api = useApi({
      apiUrl: this.config.apiUrl,
      intercept: async (url, request) => {
        if (import.meta.env.DEV) {
          await until(this.health.backendReady).toBe(true)
        }

        // Wait for token refresh if needed (unless auth: false is passed)
        if (request.auth !== false) {
          const isValid = await this.tokenValidator.ensureValidToken()

          if (!isValid) {
            throw new Error('Session expired')
          }
        }

        const accessToken = this.profile.accessToken.value
        if (accessToken && !request.headers?.['Authorization']) {
          request.headers ??= {}
          request.headers['Authorization'] = `Bearer ${accessToken}`
        }

        return request
      }
    })
  }

  get apiUrl() {
    return this.config.apiUrl
  }

  get get() {
    return this.api.get
  }

  get post() {
    return this.api.post
  }

  get put() {
    return this.api.put
  }

  get delete() {
    return this.api.delete
  }

  signalr<TSender extends SignalrSender, TReciever extends SignalrReciever>(url: string) {
    return signalr<TSender, TReciever>(this.config.apiUrl + url, () => this.profile?.accessToken?.value)
  }

  useSignalr<TSender extends SignalrSender, TReciever extends SignalrReciever>(url: string) {
    return useSignalr<TSender, TReciever>(this.config.apiUrl + url, () => this.profile?.accessToken?.value)
  }
}