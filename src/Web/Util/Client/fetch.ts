import { Ref } from 'vue'

// RFC7807 Problem Details
export interface ProblemDetails {
  type?: string
  title?: string
  status?: number
  detail?: string
  instance?: string
  // Allow additional extension members such as `exception`
  [key: string]: any
}

export class HttpError extends Error {
  status?: number
  method?: string
  url?: string
  isNetworkError?: boolean
  problemDetails?: ProblemDetails

  constructor(message: string, name: string) {
    super(message)
    this.name = name
  }
}

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
          params.append(key, String(item))
        }

        continue
      }

      if (value === undefined || value === null) {
        continue
      }

      if (typeof value === 'string') {
        for (const item of value.split(',')) {
          params.append(key, item)
        }
      }
      else {
        // Coerce numbers, booleans, dates, etc. to string
        params.append(key, String(value))
      }
    }

    url += "?" + params
  }

  if (init.isLoading) {
    init.isLoading.value = true
  }

  let response: Response
  try {
    response = await window.fetch(url, init)
  } catch (e) {
    const err = new HttpError(e instanceof Error ? e.message : String(e), 'Network Error')

    err.method = init.method
    err.url = String(url)
    err.status = 0
    err.isNetworkError = true

    throw err
  }

  if (init.isLoading) {
    init.isLoading.value = false
  }

  if (!response.ok) {
    const contentType = response.headers.get('content-type')

    const error = new HttpError(`${init.method ?? 'GET'} ${response.url} ${response.status}`, 'HTTP Error')

    // Attach HTTP context
    error.status = response.status
    error.method = init.method ?? 'GET'
    error.url = response.url

    // Attach problem details if present
    const isProblem = contentType?.indexOf('application/problem+json') >= 0
    const problemDetails = (isProblem && await response.json()) as ProblemDetails

    if (problemDetails) {
      const name = problemDetails?.title || 'Request failed'
      const message = problemDetails?.detail || problemDetails?.message || 'The server reported a problem.'

      error.name = name
      error.message = message

      if (problemDetails?.exception?.details && error.stack) {
        error.stack = problemDetails.exception.details + "\n\n" + error.stack
      }
      error.problemDetails = problemDetails
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

type RequestInterceptor = (init: RequestInitExtended) => RequestInitExtended | Promise<RequestInitExtended>

export const useApi = ({ apiUrl, intercept = x => x }: { apiUrl: string, intercept?: RequestInterceptor }) => {
  return {
    url: apiUrl,
    fetch: async (url: RequestInfo | URL, init: RequestInitExtended = {}) => fetch(apiUrl + url, await intercept(init)),
    get: async <T>(url: RequestInfo | URL, init?: RequestInitExtended) => {
      const response = await fetch(apiUrl + url, await intercept({
        method: 'GET',
        ...init
      }))

      return await parseResponse<T>(response)
    },
    post: async <T>(url: RequestInfo | URL, body?: any, init?: RequestInitExtended) => {
      const isFormData = body instanceof FormData
      const response = await fetch(apiUrl + url, await intercept({
        method: 'POST',
        body: isFormData ? body : JSON.stringify(body),
        headers: isFormData ? {} : {
          'Content-Type': 'application/json'
        },
        ...init
      }))

      return await parseResponse<T>(response)
    },
    put: async <T>(url: RequestInfo | URL, body?: any, init?: RequestInitExtended) => {
      const response = await fetch(apiUrl + url, await intercept({
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
      const response = await fetch(apiUrl + url, await intercept({
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