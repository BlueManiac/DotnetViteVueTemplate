import { computedAsync } from "@vueuse/core"
import { inject, watchEffect } from "vue"
import { useRoute } from "vue-router"
import { ApiService } from "../ApiService"
import { Profile } from "./Profile"

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
  api = inject(ApiService)
  profile = inject(Profile)

  providers = computedAsync(async () => {
    const response = await this.api.get<{ providers: string[] }>('/auth/providers')
    return response.providers
  }, [], { lazy: true })

  private refreshTimeout?: ReturnType<typeof setTimeout>

  // Watch for token changes to load profile and start refresh
  private _ = watchEffect(() => {
    if (this.profile.accessToken.value && !this.profile.user.value) {
      this.loadUserProfile()
    }
    this.startBackgroundRefresh()
  })

  private async loadUserProfile() {
    try {
      const user = await this.api.get<UserResponse>('/auth/user')
      this.profile.setUser(user)
    } catch {
      // Failed to load profile, ignore
    }
  }

  async login(username: string, password: string) {
    const response = await this.api.post<AccessTokenResponse>('/auth/login', { username, password })
    this.profile.setAuthTokens(response)
  }

  async logout() {
    this.profile.clear()
  }

  private async refresh() {
    try {
      const response = await this.api.post<AccessTokenResponse>('/auth/refresh', { refreshToken: this.profile.refreshToken.value })
      this.profile.setAuthTokens(response)
    }
    catch {
      this.profile.clear()
      throw new Error('Could not refresh token')
    }
  }

  private getRefreshDelay() {
    const timeUntilExpiry = this.profile.expiresAt.value - Date.now()

    // Refresh the token 5 seconds before it expires
    return Math.max(0, timeUntilExpiry - 5000)
  }

  private startBackgroundRefresh() {
    if (this.refreshTimeout) {
      clearTimeout(this.refreshTimeout)
    }

    if (!this.profile.isLoggedIn.value) {
      return
    }

    this.refreshTimeout = setTimeout(async () => {
      if (this.profile.isLoggedIn.value) {
        await this.refresh()
        this.startBackgroundRefresh()
      }
    }, this.getRefreshDelay())
  }
}

/**
 * Composable to handle authentication callback with tokens in URL query parameters.
 * Works with any authentication method that redirects back with accessToken, expiresIn, refreshToken, and tokenType.
 */
export function useAuthCallback(onSuccess?: () => void) {
  const authService = inject(AuthService)
  const route = useRoute()

  const { accessToken, expiresIn, refreshToken, tokenType } = route.query as Record<keyof AccessTokenResponse, string>

  if (accessToken && expiresIn && refreshToken) {
    authService.profile.setAuthTokens({
      accessToken,
      expiresIn: parseInt(expiresIn),
      refreshToken,
      tokenType: tokenType || 'Bearer'
    })

    onSuccess?.()
  }
}
