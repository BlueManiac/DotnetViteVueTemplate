/**
 * Handles server side browser refresh on backend restart during development.
 */
export function setupBrowserRefresh() {
  // Intercept the dotnet-watch socket (root URL, e.g. wss://localhost:52616/) before the
  // aspnetcore script loads it. Rude edits trigger a restart message that the aspnetcore
  // script ignores — we catch it and reload once the backend is back up.
  const OriginalWebSocket = window.WebSocket
  window.WebSocket = class extends OriginalWebSocket {
    constructor(url: string | URL, protocols?: string | string[]) {
      super(url as string, protocols)
      if (/^wss?:\/\/localhost(:\d+)?\/?$/.test(url.toString())) {
        this.addEventListener('message', (event: MessageEvent) => {
          try {
            const data = JSON.parse(event.data as string)
            if (data.type === 'HotReloadDiagnosticsv1' &&
              data.diagnostics?.some((message: string) => message == 'Restarting application to apply changes ...')) {
              location.reload()
            }
          } catch { /* ignore JSON parse errors */ }
        })
      }
    }
  }

  const script = document.createElement('script')
  script.src = `${import.meta.env.VITE_BACKEND_URL}/_framework/aspnetcore-browser-refresh.js`
  document.body.appendChild(script)

  // Workaround for dotnet compile error overlay staying visible when there are no errors
  const style = document.createElement('style')
  style.textContent = '#dotnet-compile-error:empty { display: none; }'
  document.head.appendChild(style)
}
