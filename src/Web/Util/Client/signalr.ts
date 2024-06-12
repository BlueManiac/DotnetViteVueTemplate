import { HubConnectionBuilder } from '@microsoft/signalr'
import { tryOnScopeDispose, useEventListener } from '@vueuse/core'
import { Ref, onMounted, ref } from 'vue'

export type SignalrSender = { [key: string]: (...rest: unknown[]) => Promise<void> }
export type SignalrReciever = { [key: string]: Ref<unknown> }

export function useSignalr<TSender extends SignalrSender = SignalrSender, TReciever extends SignalrReciever = SignalrReciever>(url: string) {
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

  const sender = new Proxy({}, {
    get(target, prop, receiver) {
      return function (...args: any[]) {
        return connection.send.apply(connection, [prop, ...args])
      }
    },
  }) as TSender

  const receiver = new Proxy({}, {
    get(target, prop: string, receiver) {
      return data(prop)
    },
  }) as TReciever

  return { connection, data, sender, receiver }
}