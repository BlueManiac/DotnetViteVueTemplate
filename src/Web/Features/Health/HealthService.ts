import { computedAsync } from '@vueuse/core'
import { AppConfig } from '../AppConfig'

export class HealthService {
  backendReady = computedAsync(
    async () => {
      const controller = new AbortController()
      const timeout = setTimeout(() => controller.abort(), 10000)

      while (!controller.signal.aborted) {
        try {
          await fetch(`${this.config.apiUrl}/health/ready`, {
            method: 'HEAD',
            signal: controller.signal
          })

          clearTimeout(timeout)
          return true
        }
        catch (error) {
          if (controller.signal.aborted) {
            break
          }
        }
      }

      // Timeout reached - allow requests anyway to avoid blocking forever
      return true
    },
    false,
    { lazy: false }
  )

  constructor(private config: AppConfig) { }
}
