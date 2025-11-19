import { inject, watchEffect } from "vue"
import { ApiService } from "../ApiService"
import { AccessTokenResponse, Profile } from "./Profile"

type UserResponse = {
  name: string
}

export class AuthService {
  api = inject(ApiService)!
  profile = inject(Profile)!

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
    this.profile.setLoginResponse({
      ...response,
      date: Date.now()
    })
  }

  async logout() {
    this.profile.clear()
  }

  private async refresh() {
    try {
      const response = await this.api.post<AccessTokenResponse>('/auth/refresh', { refreshToken: this.profile.loginResponse.value.refreshToken })
      this.profile.setLoginResponse({
        ...response,
        date: Date.now()
      })
    }
    catch {
      this.profile.clear()
      throw new Error('Could not refresh token')
    }
  }

  private getRefreshDelay() {
    const elapsedTime = Date.now() - this.profile.loginResponse.value.date

    // Refresh the token 5 seconds before it expires
    return this.profile.loginResponse.value.expiresIn * 1000 - elapsedTime - 5000
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