import { Ref } from 'vue'

type RequestInitExtended = RequestInit & {
  isLoading?: Ref<boolean>,
  query?: object
}

export const fetch = async (url: RequestInfo | URL, init: RequestInitExtended) => {
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

  if (init.isLoading) {
    init.isLoading.value = true
  }
  const response = await window.fetch(url, init)
  if (init.isLoading) {
    init.isLoading.value = false
  }

  if (!response.ok) {
    const contentType = response.headers.get('content-type')
    
    const error = new Error(`${init.method ?? 'GET'} ${response.url} ${response.status}`)
    
    const problemDetails = contentType?.indexOf('application/problem+json') >= 0 && await response.json()

    if (problemDetails?.exception?.details) {
      error.stack = problemDetails.exception.details + "\n\n" + error.stack
    }

    throw error
  }

  return response
}

const parseResponse = async<T>(response: Response) => {
  const contentType = response.headers.get('content-type')

  if (contentType && contentType.indexOf('application/json') >= 0) {
    return response.json() as T
  }

  return response.text() as T
}

type RequestInterceptor = (init: RequestInitExtended) => RequestInitExtended

export const useApi = ({ apiUrl, intercept = x => x }: { apiUrl: string, intercept?: RequestInterceptor}) => {
  return {
    url: apiUrl,
    fetch: (url: RequestInfo | URL, init: RequestInitExtended = {}) => fetch(apiUrl + url, intercept(init)),
    get: async <T>(url: RequestInfo | URL, init?: RequestInitExtended) => {
      const response = await fetch(apiUrl + url, intercept({
        method: 'GET',
        ...init
      }))

      return await parseResponse<T>(response)
    },
    post: async <T>(url: RequestInfo | URL, body?: any, init?: RequestInitExtended) => {
      const response = await fetch(apiUrl + url, intercept({
        method: 'POST',
        body: JSON.stringify(body),
        headers: {
          'Content-Type': 'application/json'
        },
        ...init
      }))

      return await parseResponse<T>(response)
    },
    put: async <T>(url: RequestInfo | URL, body?: any, init?: RequestInitExtended) => {
      const response = await fetch(apiUrl + url, intercept({
        method: 'PUT',
        body: JSON.stringify(body),
        headers: {
          'Content-Type': 'application/json'
        },
        ...init
      }))

      return await parseResponse<T>(response)
    },
    delete: async <T>(url: RequestInfo | URL, body?: any, init?: RequestInitExtended) => {
      const response = await fetch(apiUrl + url, intercept({
        method: 'DELETE',
        body: JSON.stringify(body),
        headers: {
          'Content-Type': 'application/json'
        },
        ...init
      }))

      return await parseResponse<T>(response)
    }
  }
}