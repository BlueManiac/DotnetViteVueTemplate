type RequestInitExtended = RequestInit & { response?: boolean }

const { fetch: originalFetch } = window
window.fetch = async (input: RequestInfo | URL, init?: RequestInitExtended) => {
  if (import.meta.env.DEV) {
    input = import.meta.env.VITE_API_URL + input
  }

  const response = await originalFetch(input, init)

  if (init?.response) {
    return response
  }

  return response.json()
};