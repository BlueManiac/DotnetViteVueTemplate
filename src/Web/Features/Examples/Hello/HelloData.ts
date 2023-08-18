import { useIntervalFn, useStorage } from '@vueuse/core'

export const rotation = ref(0)
export const speed = useStorage('speed', 1)

export const { isActive, pause, resume } = useIntervalFn(() => {
  rotation.value = (rotation.value + speed.value) % 360
}, 50)

export type Hello = { hello: string }

export const isLoading = ref(false)

export const load = () => api.get<Hello>('/api/hello', { isLoading })