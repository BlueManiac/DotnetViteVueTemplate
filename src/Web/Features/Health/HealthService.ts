import { computedAsync } from '@vueuse/core'
import { AppConfig } from '../AppConfig'

export class HealthService {
  backendReady = computedAsync(
    async () => {
      const eventSource = new EventSource(`${this.config.apiUrl}/health/ready`)

      await new Promise<void>((resolve, reject) => {
        const timeout = setTimeout(() => {
          eventSource.close()
          reject(new Error('Backend ready timeout'))
        }, 10000)

        eventSource.addEventListener('message', () => {
          clearTimeout(timeout)
          eventSource.close()
          resolve()
        })
      })

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
