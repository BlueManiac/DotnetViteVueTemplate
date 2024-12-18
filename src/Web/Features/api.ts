import { accessToken } from './Auth/AuthenticationService'
import { apiUrl } from './info'
import { useApi } from '/Util/Client/fetch'
import { signalr, SignalrReciever, SignalrSender, useSignalr } from '/Util/Client/signalr'

export const api = {
  url: apiUrl,
  ...useApi({
    apiUrl,
    intercept: request => {
      if (accessToken.value && (!request.headers || !request.headers['Authorization'])) {
        request.headers ??= {}
        request.headers['Authorization'] = `Bearer ${accessToken.value}`
      }

      return request
    }
  }),
  signalr<TSender extends SignalrSender = SignalrSender, TReciever extends SignalrReciever = SignalrReciever>(url: string) {
    return signalr<TSender, TReciever>(apiUrl + url)
  },
  useSignalr<TSender extends SignalrSender = SignalrSender, TReciever extends SignalrReciever = SignalrReciever>(url: string) {
    return useSignalr<TSender, TReciever>(apiUrl + url)
  },
}