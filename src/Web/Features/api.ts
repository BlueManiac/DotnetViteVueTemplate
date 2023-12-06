import { accessToken } from './Auth/AuthenticationService'
import { apiUrl } from './info'
import { useApi } from '/Util/Client/fetch'
import { useSignalr } from '/Util/Client/signalr'

export const api = {
  url: apiUrl,
  ...useApi({
    apiUrl,
    intercept: request =>  {
      if (accessToken.value && (!request.headers || !request.headers['Authorization'])) {
        request.headers ??= {}
        request.headers['Authorization'] = `Bearer ${accessToken.value}`
      }

      return request
    }
  }),
  signalr: (url: string) => useSignalr(apiUrl + url)
}