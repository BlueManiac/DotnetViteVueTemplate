<template>
  <div class="position-fixed top-0 end-0" style="z-index: 1100;">
    <div class="d-flex flex-column gap-2">
      <div v-for="item in notifications" :key="item.id" class="alert fade show" role="alert" :class="variantClass(item)">
        <div class="d-flex justify-content-between gap-3">
          <strong>{{ item.title || label(item) }}</strong>
          <button type="button" class="btn-close" aria-label="Close" @click="dismissNotification(item.id)"></button>
        </div>
        <div v-if="item.detail" class="mt-1">{{ item.detail }}</div>
        <div v-if="item.status || item.method || item.url" class="mt-1 small text-muted d-flex flex-wrap gap-2">
          <span v-if="item.status">HTTP {{ item.status }}</span>
          <span v-if="item.method">{{ item.method }}</span>
          <span v-if="item.url" class="text-break">{{ item.url }}</span>
        </div>
        <div v-if="item.data" class="mt-2">
          <button type="button" class="btn btn-sm btn-outline-secondary mb-2 rounded-0" @click="toggleData(item.id)">
            <span v-if="isExpanded(item.id)">Hide information</span>
            <span v-else>Show more information</span>
          </button>
          <table v-if="isExpanded(item.id)" class="table table-sm mb-0 align-middle border" style="overflow: auto;">
            <tbody>
              <tr v-for="{ key, value, primitive } in item.data" :key="key">
                <th class="text-nowrap">{{ key }}</th>
                <td>
                  <template v-if="primitive">{{ value }}</template>
                  <template v-else>
                    <code class="small clamp">{{ value }}</code>
                  </template>
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref } from 'vue'
import { dismissNotification, NotificationEntry, notifications } from './notifications'

const label = (item: NotificationEntry) => {
  switch (item.type) {
    case 'success': return 'Success'
    case 'warning': return 'Warning'
    case 'error': return 'Error'
    default: return 'Info'
  }
}

const variantClass = (item: NotificationEntry) => {
  switch (item.type) {
    case 'success': return 'alert-success'
    case 'warning': return 'alert-warning'
    case 'error': return 'alert-danger'
    default: return 'alert-info'
  }
}

// Track which notifications that have their data expanded
const expandedIds = ref<Set<number>>(new Set())

function isExpanded(id: number) {
  return expandedIds.value.has(id)
}

function toggleData(id: number) {
  if (expandedIds.value.has(id)) {
    expandedIds.value.delete(id)
  } else {
    expandedIds.value.add(id)
  }
  // Force reactivity update for Set mutations
  expandedIds.value = new Set(expandedIds.value)
}
</script>

<style scoped>
.alert {
  min-width: 22rem;
  max-width: 60rem;
}

.data-container {
  background: var(--bs-body-bg);
}

table th {
  font-weight: 600;
}

.clamp {
  display: -webkit-box;
  -webkit-box-orient: vertical;
  line-clamp: 3;
  -webkit-line-clamp: 3;
  overflow: hidden;
}
</style>
