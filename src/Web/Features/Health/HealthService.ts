import { computedAsync } from '@vueuse/core'
import { AppConfig } from '../AppConfig'

export class HealthService {
  backendReady = computedAsync(
    async () => {
      const controller = new AbortController()
      const timeout = setTimeout(() => controller.abort(), 10000)

      await fetch(`${this.config.apiUrl}/health/ready`, {
        method: 'HEAD',
        signal: controller.signal
      })

      clearTimeout(timeout)

      return true
    },
    false,
    {
      lazy: false,
      // Timeout reached - allow requests anyway to avoid blocking forever
      onError: () => true
    }
  )

  constructor(private config: AppConfig) { }
}
