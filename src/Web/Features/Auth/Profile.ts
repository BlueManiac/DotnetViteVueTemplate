import { useLocalStorage } from "@vueuse/core"
import { watch } from "vue"
import { AccessTokenResponse } from "./AuthService"

type UserData = {
  name: string
}

type AuthData = {
  accessToken?: string
  refreshToken?: string
  expiresAt?: number
  tokenType?: string
}

export class Profile {
  user = ref<UserData | null>(null)

  private authData = useLocalStorage<AuthData>('auth-data', {})

  expiresAt = computed(() => this.authData.value.expiresAt ?? 0)
  isLoggedIn = computed(() => this.authData.value.accessToken && Date.now() < this.expiresAt.value)

  accessToken = computed(() => this.authData.value.accessToken)
  refreshToken = computed(() => this.authData.value.refreshToken)

  userName = computed(() => this.user.value?.name ?? null)

  constructor() {
    // Clear user data when auth data is cleared (e.g., logout in another tab)
    watch(() => this.authData.value.accessToken, (token) => {
      if (!token) {
        this.user.value = null
      }
    })
  }

  setUser(user: UserData | null) {
    this.user.value = user
  }

  setAuthTokens(response: AccessTokenResponse) {
    this.authData.value = {
      accessToken: response.accessToken,
      refreshToken: response.refreshToken,
      expiresAt: Date.now() + response.expiresIn * 1000,
      tokenType: response.tokenType
    }
  }

  clear() {
    this.authData.value = {}
    this.user.value = null
  }
}