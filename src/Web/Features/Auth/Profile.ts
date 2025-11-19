import { useLocalStorage } from "@vueuse/core"

type UserResponse = {
  name: string
}

export interface AccessTokenResponse {
  accessToken?: string
  refreshToken?: string
  expiresIn?: number
  tokenType?: string
  date?: number
}

export class Profile {
  user = ref<UserResponse | null>(null)
  loginResponse = useLocalStorage<AccessTokenResponse>('auth-login', {})

  expiresAt = computed(() => this.loginResponse.value.date + this.loginResponse.value.expiresIn * 1000)
  isLoggedIn = computed(() => this.loginResponse.value.accessToken && Date.now() < this.expiresAt.value)

  accessToken = computed(() => this.loginResponse.value.accessToken)
  userName = computed(() => this.user.value?.name ?? null)

  setUser(user: UserResponse | null) {
    this.user.value = user
  }

  setLoginResponse(response: AccessTokenResponse) {
    this.loginResponse.value = response
  }

  clear() {
    this.user.value = null
    this.loginResponse.value = {}
  }
}