import { computedAsync } from "@vueuse/core"
import type { InjectionKey } from "vue"
import { inject, watch } from "vue"
import { useRoute } from "vue-router"
import { NotificationService } from "../Infrastructure/Notifications/notifications"
import { Profile } from "./Profile"
import { TokenValidator } from "./TokenValidator"
import { ApiService } from "/ApiService"

type UserResponse = {
  name: string
  email?: string
  provider?: string
}

export interface AccessTokenResponse {
  accessToken: string
  refreshToken: string
  expiresIn: number
  tokenType: string
}

export class AuthService {
  static readonly token: InjectionKey<AuthService> = Symbol(AuthService.name)

  private api: ApiService
  private profile: Profile
  private notifications: NotificationService
  private tokenValidator: TokenValidator

  providers = computedAsync(async () => {
    const data = await this.api.get<{ providers: string[] }>('/auth/providers', { auth: false })
    return data.providers
  }, [], { lazy: true })

  constructor(
    tokenValidator: TokenValidator,
    api: ApiService,
    profile: Profile,
    notifications: NotificationService
  ) {
    this.api = api
    this.profile = profile
    this.notifications = notifications
    this.tokenValidator = tokenValidator

    // Set refresh callback for token validator
    this.tokenValidator.setRefreshCallback(() => this.refresh())

    // Start background refresh and load profile at login or initial load when already logged in
    watch(this.profile.isLoggedIn, (isLoggedIn) => {
      if (isLoggedIn) {
        this.tokenValidator.startBackgroundRefresh()
        this.ensureUserProfileLoaded()
      }
    }, { immediate: true })
  }

  async refresh(): Promise<void> {
    const refreshToken = this.profile.refreshToken.value
    if (!refreshToken) {
      this.profile.clear()
      return
    }

    try {
      console.log('Refreshing access token...')
      const data = await this.api.post<AccessTokenResponse>('/auth/refresh', { refreshToken }, { auth: false })
      this.profile.setAuthTokens(data)
    } catch (error) {
      // Refresh token expired - log out and notify user
      this.profile.clear()
      this.notifications.notify({
        type: 'warning',
        title: 'Session expired',
        message: 'Please log in again.',
      })
    }
  }

  async ensureUserProfileLoaded() {
    if (this.profile.user.value)
      return

    try {
      this.profile.user.value = await this.api.get<UserResponse>('/auth/user')
    } catch {
      // User profile load failed, ignore
    }
  }

  async logout() {
    try {
      await this.api.post('/auth/logout', undefined, { auth: false })
    } finally {
      this.profile.clear()
    }
  }
}

/**
 * Composable to handle authentication callback with tokens in URL query parameters.
 * Works with any authentication method that redirects back with accessToken, expiresIn, refreshToken, and tokenType.
 */
export function useAuthCallback(onSuccess?: () => void) {
  const profile = inject(Profile.token)!
  const authService = inject(AuthService.token)!
  const route = useRoute()

  const { accessToken, expiresIn, refreshToken, tokenType } = route.query as Record<keyof AccessTokenResponse, string>

  if (accessToken && expiresIn && refreshToken) {
    profile.setAuthTokens({
      accessToken,
      expiresIn: parseInt(expiresIn),
      refreshToken,
      tokenType: tokenType || 'Bearer'
    })

    onSuccess?.()
  }
}
