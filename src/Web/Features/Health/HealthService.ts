import { computedAsync } from '@vueuse/core'
import { AppConfig } from '../AppConfig'
import { NotificationService } from '/Components/Notifications/notifications'

export class HealthService {
  private static readonly INITIAL_RETRY_DELAY = 500
  private static readonly MAX_RETRY_DELAY = 3000
  private static readonly REQUEST_TIMEOUT = 5000

  backendReady = computedAsync(
    async () => {
      let retryDelay = HealthService.INITIAL_RETRY_DELAY
      let attemptCount = 0
      let notificationId: number | undefined

      while (true) {
        try {
          const controller = new AbortController()
          const timeout = setTimeout(() => controller.abort(), HealthService.REQUEST_TIMEOUT)

          await fetch(`${this.config.apiUrl}/health/ready`, {
            method: 'HEAD',
            signal: controller.signal
          })

          clearTimeout(timeout)

          // Dismiss the notification when connection is restored
          if (notificationId !== undefined) {
            this.notifications.dismiss(notificationId)
          }

          return true
        }
        catch (error) {
          attemptCount++

          // Show notification on first failed attempt
          if (attemptCount === 1) {
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
    },
    false,
    { lazy: false }
  )

  constructor(
    private config: AppConfig,
    private notifications: NotificationService
  ) { }
}
