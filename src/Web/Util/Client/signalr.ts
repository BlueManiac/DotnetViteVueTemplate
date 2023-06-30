import { tryOnScopeDispose, useEventListener } from '@vueuse/core'
import { HubConnectionBuilder } from '@microsoft/signalr'
import { onMounted, ref } from 'vue'

export function useSignalr(url: string) {
  const connection = new HubConnectionBuilder()
    .withUrl(url)
    .withAutomaticReconnect()
    .build()

  onMounted(() => {
    connection.start()
  })

  const close = () => {
    connection.stop()
  }

  useEventListener(window, 'beforeunload', () => close())
  tryOnScopeDispose(close)

  const data = function <T>(methodName: string) {
    const val = ref<T>()

    connection.on(methodName, (data: T) => {
      val.value = data
    })

    return val
  }

  return { connection, data }
}