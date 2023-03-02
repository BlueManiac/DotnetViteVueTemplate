export { }
declare global {
  type RequestInitExtended = RequestInit & { apiUrl?: string }
  interface TypedResponse<T> extends Response {
    json(): Promise<T>
  }

  function fetch(input: RequestInfo | URL, init?: RequestInitExtended): Promise<Response>
  function fetch<T>(input: RequestInfo | URL, init?: RequestInitExtended): Promise<TypedResponse<T>>

  var apiUrl: string
  function get<T>(input: RequestInfo | URL, init?: RequestInitExtended): Promise<T>
  function get(input: RequestInfo | URL, init?: RequestInitExtended): Promise<string>
  function post<T>(input: RequestInfo | URL, data: any, init?: RequestInitExtended): Promise<T>
  function post(input: RequestInfo | URL, data: any, init?: RequestInitExtended): Promise<string>
  function put<T>(input: RequestInfo | URL, data: any, init?: RequestInitExtended): Promise<T>
  function put(input: RequestInfo | URL, data: any, init?: RequestInitExtended): Promise<string>
}

const { fetch: originalFetch } = window
window.fetch = (input: RequestInfo | URL, init?: RequestInitExtended) => {
  if (init?.apiUrl) {
    input = init.apiUrl + input
  }

  return originalFetch(input, init)
}

window.get = function <T>(input: RequestInfo | URL, init: RequestInitExtended = {}) {
  init.apiUrl ??= apiUrl
  
  return processFetch<T>(input, init)
}

window.post = function <T>(input: RequestInfo | URL, data?: any, init: RequestInitExtended = {}) {
  init.apiUrl ??= apiUrl
  init.method = 'POST'
  init.headers ??= {
    'accept': 'application/json',
    'content-type': 'application/json'
  }
  init.body ??= JSON.stringify(data)

  return processFetch<T>(input, init)
}

window.put = function <T>(input: RequestInfo | URL, data?: any, init: RequestInitExtended = {}) {
  init.apiUrl ??= apiUrl
  init.method = 'PUT'
  init.headers ??= {
    'accept': 'application/json',
    'content-type': 'application/json'
  }
  init.body ??= JSON.stringify(data)

  return processFetch<T>(input, init)
}

const processFetch = async function <T>(input: RequestInfo | URL, init?: RequestInitExtended) {
  const response = await fetch<T>(input, init)

  const contentType = response.headers.get("content-type")

  if (!response.ok) {
    throw {
      url: response.url,
      status: response.status,
      method: init?.method ?? 'GET',
      problemDetails: contentType?.indexOf("application/problem+json") >= 0
        ? await response.json()
        : null
    }
  }

  if (contentType?.indexOf("application/json") >= 0) {
    return await response.json()
  }

  return await response.text()
}