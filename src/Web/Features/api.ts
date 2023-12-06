import { apiUrl } from './info'
import { useApi } from '/Util/Client/fetch'
import { useSignalr } from '/Util/Client/signalr'

export const api = {
  url: apiUrl,
  ...useApi({
    apiUrl
  }),
  signalr: (url: string) => useSignalr(apiUrl + url)
}