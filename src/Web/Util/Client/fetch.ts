import { Ref } from 'vue'

type RequestInitExtended = RequestInit & {
  isLoading?: Ref<boolean>,
  query?: object
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
    
    const error = new Error( `${init.method ?? 'GET'} ${response.url} ${response.status}`)
    
    const problemDetails = contentType?.indexOf('application/problem+json') >= 0 && await response.json()

    if (problemDetails?.exception?.details) {
      error.stack = problemDetails.exception.details + "\n\n" + error.stack
    }

    throw error
  }

  return response
}

export const useApi = ({ apiUrl }) => {
  return {
    url: apiUrl,
    fetch: (url: RequestInfo | URL, init: RequestInitExtended = {}) => fetch(apiUrl + url, init),
    get: async <T>(url: RequestInfo | URL, init?: RequestInitExtended) => {
      if (init?.query) {
        const params = new URLSearchParams()

        for (const [key, value] of Object.entries(init.query)) {
          if (Array.isArray(value)) {
            for (const item of value) {
              params.append(key, item)
            }

            continue
          }

          for (const item of value.split(',')) {
            params.append(key, item)
          }
        }

        url += "?" + params
      }

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