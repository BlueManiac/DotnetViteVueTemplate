import { until } from '@vueuse/core'
import type { InjectionKey } from 'vue'
import { ref, watch } from 'vue'
import { NotificationService } from '../Notifications/notifications'
import { AppConfig } from '/Util/AppConfig'

export class HealthService {
  static readonly token: InjectionKey<HealthService> = Symbol(HealthService.name)

  private static readonly INITIAL_RETRY_DELAY = 500
  private static readonly MAX_RETRY_DELAY = 3000
  private static readonly REQUEST_TIMEOUT = 5000

  backendReady = ref(false)

  constructor(
    private config: AppConfig,
    private notifications: NotificationService
  ) {
    watch(this.backendReady, async (ready) => {
      if (!ready) {
        await this.retryUntilReady()
        this.backendReady.value = true
      }
    }, { immediate: true })
  }

  waitUntilReady(): Promise<boolean> {
    return until(this.backendReady).toBe(true)
  }

  // Triggers a new health check (e.g. after detecting a backend restart)
  reset() {
    this.backendReady.value = false
  }

  private async retryUntilReady(): Promise<void> {
    let retryDelay = HealthService.INITIAL_RETRY_DELAY
    let notificationId: number | undefined

    while (true) {
      try {
        const controller = new AbortController()
        const timeout = setTimeout(() => controller.abort(), HealthService.REQUEST_TIMEOUT)
        try {
          await fetch(`${this.config.apiUrl}/health/ready`, { method: 'HEAD', signal: controller.signal })
        } finally {
          clearTimeout(timeout)
        }

        if (notificationId !== undefined) {
          // Dismiss the notification when connection is restored
          this.notifications.dismiss(notificationId)
        }

        return
      }
      catch (error) {
        if (notificationId === undefined) {
          // Show notification on first failed attempt
          if (error instanceof TypeError && error.message.includes('Failed to fetch')) {
            console.warn('Health check failed - network or connectivity issue. Retrying with backoff...')
          }

          notificationId = this.notifications.notify({
            type: 'warning',
            title: 'Backend Connection Failed',
            message: 'Could not connect to the backend. Waiting for it to become available...',
            persistent: true,
          })
        }

        // Exponential backoff - wait before next retry
        await new Promise(resolve => setTimeout(resolve, retryDelay))
        retryDelay = Math.min(retryDelay * 2, HealthService.MAX_RETRY_DELAY)
      }
    }
  }
}
