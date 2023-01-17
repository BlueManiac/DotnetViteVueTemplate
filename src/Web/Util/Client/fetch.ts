type RequestInitExtended = RequestInit & { response?: boolean }

export function setupFetch(apiUrl?: string) {
  const { fetch: originalFetch } = window
  window.fetch = async (input: RequestInfo | URL, init?: RequestInitExtended) => {
    if (apiUrl) {
      input = apiUrl + input
    }

    const response = await originalFetch(input, init)

    if (init?.response) {
      return response
    }

    if (!response.ok) {
      throw await response.json()
    }

    return response.json()
  }
}