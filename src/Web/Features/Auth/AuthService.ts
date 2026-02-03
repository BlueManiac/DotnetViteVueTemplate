import { computedAsync } from "@vueuse/core"
import { inject, watchEffect } from "vue"
import { useRoute } from "vue-router"
import { NotificationService } from "../../Components/Notifications/notifications"
import { ApiService } from "../ApiService"
import { Profile } from "./Profile"
import { TokenValidator } from "./TokenValidator"

type UserResponse = {
  name: string
}

export interface AccessTokenResponse {
  accessToken: string
  refreshToken: string
  expiresIn: number
  tokenType: string
}

export class AuthService {
  private api: ApiService
  private profile: Profile
  private notifications: NotificationService
  private tokenValidator: TokenValidator

  providers = computedAsync(async () => {
    const response = await window.fetch(this.api.apiUrl + '/auth/providers')
    const data = await response.json() as { providers: string[] }
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

    // Start background refresh on token changes
    watchEffect(() => {
      if (this.profile.isLoggedIn.value) {
        this.tokenValidator.startBackgroundRefresh()
      }
    })
  }

  async refresh(): Promise<void> {
    const refreshToken = this.profile.refreshToken.value
    if (!refreshToken) {
      this.profile.clear()
      return
    }

    try {
      // Use window.fetch directly to avoid circular dependency with API interceptor
      const response = await window.fetch(this.api.apiUrl + '/auth/refresh', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify({ refreshToken }),
      })

      if (!response.ok) {
        // Refresh token expired - log out and notify user
        this.profile.clear()
        this.notifications.notify({
          type: 'warning',
          title: 'Session expired',
          detail: 'Please log in again.',
        })
        return
      }

      const data = await response.json() as AccessTokenResponse
      this.profile.setAuthTokens(data)

      // Load user profile after successful refresh
      await this.ensureUserProfileLoaded()
    } catch (error) {
      this.notifications.notifyError(error as Error)
      this.profile.clear()
    }
  }

  private async loadUserProfile() {
    try {
      const user = await this.api.get<UserResponse>('/auth/user')
      this.profile.user.value = user
    } catch {
      // User profile load failed, ignore
    }
  }

  async ensureUserProfileLoaded() {
    if (!this.profile.user.value) {
      await this.loadUserProfile()
    }
  }

  async logout() {
    try {
      await this.api.post('/auth/logout')
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
  const profile = inject(Profile)
  const authService = inject(AuthService)
  const route = useRoute()

  const { accessToken, expiresIn, refreshToken, tokenType } = route.query as Record<keyof AccessTokenResponse, string>

  if (accessToken && expiresIn && refreshToken) {
    profile.setAuthTokens({
      accessToken,
      expiresIn: parseInt(expiresIn),
      refreshToken,
      tokenType: tokenType || 'Bearer'
    })

    // Load user profile after successful login (don't await - let it load in background)
    authService.ensureUserProfileLoaded()

    onSuccess?.()
  }
}
