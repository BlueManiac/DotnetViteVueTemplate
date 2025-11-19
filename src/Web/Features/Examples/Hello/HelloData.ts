import { useIntervalFn, useStorage } from '@vueuse/core'
import { inject } from 'vue'
import { ApiService } from '../../ApiService'

export const useHelloData = () => {
  const api = inject(ApiService)

  const rotation = ref(0)
  const speed = useStorage('speed', 1)

  const { isActive, pause, resume } = useIntervalFn(() => {
    rotation.value = (rotation.value + speed.value) % 360
  }, 50)

  const isLoading = ref(false)

  const load = () => api.get<Hello>('/api/hello', { isLoading })

  return {
    rotation,
    speed,
    isActive,
    pause,
    resume,
    isLoading,
    load
  }
}

export type Hello = { hello: string }