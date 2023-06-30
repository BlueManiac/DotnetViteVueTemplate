import { Ref } from 'vue'

type RequestInitExtended = RequestInit & {
  isLoading?: Ref<boolean>
}

export const fetch = async (url: RequestInfo | URL, init: RequestInitExtended) => {
  if (init.isLoading) {
    init.isLoading.value = true
  }
  const response = await window.fetch(url, init)
  if (init.isLoading) {
    init.isLoading.value = false
  }

  if (!response.ok) {
    const contentType = response.headers.get('content-type')

    throw {
      url: response.url,
      body: init.body,
      status: response.status,
      method: init?.method ?? 'GET',
      problemDetails: contentType?.indexOf('application/problem+json') >= 0
        ? await response.json()
        : undefined
    }
  }

  return response
}

export const useApi = ({ apiUrl }) => {
  return {
    url: apiUrl,
    fetch: (url: RequestInfo | URL, init: RequestInitExtended = {}) => fetch(apiUrl + url, init),
    get: async <T>(url: RequestInfo | URL, init?: RequestInitExtended) => {
      const response = await fetch(apiUrl + url, { method: 'GET', ...init })

      const json = await response.json()

      return json as T
    },
    post: async <T>(url: RequestInfo | URL, body?: any, init?: RequestInitExtended) => {
      const response = await fetch(apiUrl + url, {
        method: 'POST',
        body: JSON.stringify(body),
        headers: {
          'Content-Type': 'application/json'
        },
        ...init
      })

      const json = await response.json()

      return json as T
    },
    put: async <T>(url: RequestInfo | URL, body?: any, init?: RequestInitExtended) => {
      const response = await fetch(apiUrl + url, {
        method: 'PUT',
        body: JSON.stringify(body),
        headers: {
          'Content-Type': 'application/json'
        },
        ...init
      })

      const json = await response.json()

      return json as T
    },
    delete: async <T>(url: RequestInfo | URL, body?: any, init?: RequestInitExtended) => {
      const response = await fetch(apiUrl + url, {
        method: 'DELETE',
        body: JSON.stringify(body),
        headers: {
          'Content-Type': 'application/json'
        },
        ...init
      })

      const json = await response.json()

      return json as T
    }
  }
}