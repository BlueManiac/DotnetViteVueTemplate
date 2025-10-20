import { ref } from 'vue'

export type NotificationType = 'info' | 'success' | 'warning' | 'error'

export interface NotificationEntry {
  id: number
  type: NotificationType
  title?: string
  detail?: string
  status?: number
  method?: string
  url?: string
  timestamp: Date
  data?: { key: string, value: any, primitive: boolean }[]
}

type RecordOrArray = Record<string, any> | { key: string, value: any }[]

export const notifications = ref<NotificationEntry[]>([])

export function dismissNotification(id: number) {
  notifications.value = notifications.value.filter(n => n.id !== id)
}

export function notify(entry: Omit<NotificationEntry, 'id' | 'data' | 'timestamp'> & { data?: RecordOrArray }) {
  const id = Date.now() + Math.floor(Math.random() * 1000)
  const type = entry.type ?? 'info'

  function formatData(data?: RecordOrArray) {
    if (!data)
      return

    function isPrimitive(val: any) {
      return val === null || ['string', 'number', 'boolean'].includes(typeof val)
    }

    return Object.entries(data)
      .map(([key, value]) => ({ key, value, primitive: isPrimitive(value) }))
      .sort((a, b) => {
        if (a.primitive === b.primitive)
          return a.key.localeCompare(b.key)

        // primitives first
        return a.primitive ? -1 : 1
      })
  }

  notifications.value.unshift({
    id,
    type,
    title: entry.title,
    detail: entry.detail,
    status: entry.status,
    method: entry.method,
    url: entry.url,
    timestamp: new Date(),
    data: formatData(entry.data),
  })

  // Auto-dismiss non-error notifications after 6s
  if (type !== 'error') {
    setTimeout(() => dismissNotification(id), 6000)
  }

  return id
}

export function notifyError(error: Error | string, ctx?: { status?: number, method?: string, url?: string }, data?: RecordOrArray) {
  const err = error as Error
  const title = err?.name ?? 'Error'
  const detail = typeof err === 'string'
    ? err
    : (err?.message || 'An unexpected error occurred')

  return notify({ type: 'error', title, detail, ...ctx, data })
}