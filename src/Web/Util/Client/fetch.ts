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
  function post<T>(input: RequestInfo | URL, data: any, init?: RequestInitExtended): Promise<T>
  function post(input: RequestInfo | URL, data: any, init?: RequestInitExtended): Promise<string>
  function put<T>(input: RequestInfo | URL, data: any, init?: RequestInitExtended): Promise<T>
  function put(input: RequestInfo | URL, data: any, init?: RequestInitExtended): Promise<string>
}

const { fetch: originalFetch } = window
window.fetch = async (input: RequestInfo | URL, init?: RequestInitExtended) => {
  if (init?.apiUrl) {
    input = init.apiUrl + input
  }

  return originalFetch(input, init)
}

window.get = async function <T>(input: RequestInfo | URL, init?: RequestInitExtended) {
  const baseUrl = init?.apiUrl ?? apiUrl

  if (baseUrl) {
    input = baseUrl + input
  }

  const response = await fetch<T>(input, init)

  if (!response.ok) {
    throw await response.json()
  }

  return await response.json()
}

window.post = async function <T>(input: RequestInfo | URL, data?: any, init?: RequestInitExtended) {
  init ??= {}

  init.method = 'POST'
  init.apiUrl ??= apiUrl
  init.headers ??= {
    'accept': 'application/json',
    'content-type': 'application/json'
  }

  init.body ??= JSON.stringify(data)

  const response = await fetch<T>(input, init)

  if (!response.ok) {
    throw await response.json()
  }

  const contentType = response.headers.get("content-type")

  if (contentType?.indexOf("application/json") >= 0) {
    return await response.json()
  }

  return response.text()
}

window.put = async function <T>(input: RequestInfo | URL, data?: any, init?: RequestInitExtended) {
  init ??= {}

  init.method = 'PUT'
  init.apiUrl ??= apiUrl
  init.headers ??= {
    'accept': 'application/json',
    'content-type': 'application/json'
  }

  init.body ??= JSON.stringify(data)

  const response = await fetch<T>(input, init)

  if (!response.ok) {
    throw await response.json()
  }

  const contentType = response.headers.get("content-type")

  if (contentType?.indexOf("application/json") >= 0) {
    return await response.json()
  }

  return response.text()
}