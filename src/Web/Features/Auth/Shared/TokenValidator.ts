import type { InjectionKey } from 'vue'

export type RefreshCallback = () => Promise<void>

export class TokenValidator {
  static readonly token: InjectionKey<TokenValidator> = Symbol(TokenValidator.name)

  private refreshPromise?: Promise<void>
  private refreshTimeout?: ReturnType<typeof setTimeout>
  private onRefresh?: RefreshCallback

  constructor(
    private getExpiresAt: () => number | undefined,
    private isLoggedIn: () => boolean
  ) { }

  setRefreshCallback(callback: RefreshCallback) {
    this.onRefresh = callback
  }

  async ensureValidToken(): Promise<boolean> {
    if (!this.isLoggedIn()) {
      return false
    }

    // If token is not expired yet (with 5 second buffer), we're good
    if (!this.isTokenExpired(5000)) {
      return true
    }

    // If refresh is already in progress, wait for it
    if (this.refreshPromise) {
      await this.refreshPromise
      return this.isLoggedIn()
    }

    // Start refresh by calling the callback if available
    if (!this.onRefresh) {
      console.warn("TokenValidator: No refresh callback set, cannot refresh token")
      return false
    }

    this.refreshPromise = this.onRefresh()
      .finally(() => {
        this.refreshPromise = undefined
      })

    await this.refreshPromise
    return this.isLoggedIn()
  }

  startBackgroundRefresh() {
    if (this.refreshTimeout) {
      clearTimeout(this.refreshTimeout)
      this.refreshTimeout = undefined
    }

    const expiresAt = this.getExpiresAt()
    if (!expiresAt || !this.onRefresh) {
      return
    }

    // Refresh 5 seconds before expiration (or immediately if already expired)
    const timeUntilRefresh = expiresAt - Date.now() - 5000
    const delay = Math.max(0, timeUntilRefresh)

    this.refreshTimeout = setTimeout(() => {
      this.ensureValidToken().then(() => this.startBackgroundRefresh())
    }, delay)
  }

  private isTokenExpired(bufferMs = 0): boolean {
    const expiresAt = this.getExpiresAt()
    if (!expiresAt) {
      return false
    }

    return Date.now() >= expiresAt - bufferMs
  }
}
