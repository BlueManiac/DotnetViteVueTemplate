import { HubConnectionBuilder, HubConnectionState } from '@microsoft/signalr'
import { tryOnScopeDispose, useEventListener } from '@vueuse/core'
import { Ref, onMounted, ref } from 'vue'

export type SignalrSender = { [key: string]: (...rest: unknown[]) => Promise<void> }
export type SignalrReciever = { [key: string]: Ref<unknown> }

export function signalr<TSender extends SignalrSender = SignalrSender, TReciever extends SignalrReciever = SignalrReciever>(url: string, accessToken?: () => string | undefined) {
  const connection = new HubConnectionBuilder()
    .withUrl(url, {
      accessTokenFactory: () => accessToken?.() ?? ''
    })
    .withAutomaticReconnect()
    .build()

  const start = async () => {
    if (connection.state !== HubConnectionState.Disconnected)
      return connection

    await connection.start()

    return connection
  }

  const stop = async () => {
    await connection.stop()
  }

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

  return { connection, data, sender, receiver, start, stop }
}

export function useSignalr<TSender extends SignalrSender = SignalrSender, TReciever extends SignalrReciever = SignalrReciever>(url: string, accessToken?: () => string | undefined) {
  const data = signalr<TSender, TReciever>(url, accessToken)

  onMounted(() => {
    data.start()
  })

  useEventListener(window, 'beforeunload', () => data.stop())
  tryOnScopeDispose(data.stop)

  return data
}