<template>
  <div>
    <div class="d-flex gap-2 align-items-center">
      <btn @click="add()">Add</btn>
      <btn @click="remove()">Remove</btn>
      <label>Quantity:</label>
      <input-number v-model="changeQuantity"></input-number>
      <input-text v-model="filterDebounced" placeholder="Filter" class="ms-auto" />
      <btn @click="filterString = ''">Clear</btn>
    </div>
    <range v-model.number="changeQuantity" min="1" max="10000" class="mt-3" />
    Quantity: {{ items.length }}, Selected: {{ selected.length }} {{ selected[0] }}, Filtered: {{ filteredItems.length }}
    <context-menu ref="contextMenuElement" />
    <data-table class="table-sm" v-model="filteredItems" :columns="visibleColumns" v-model:selected="selected" v-model:sortField="sortField" v-model:sortOrder="sortOrder" v-model:filter="filter" @headerContextMenuClick="onHeaderContextMenu" @rowContextMenuClick="onRowContextMenu">
      <template #id="{ item, col }">
        {{ item[col.field] }}
      </template>
      <template #color="{ item }">
        <div class="px-2" :style="{ 'background-color': item.color, 'color': invertColor(item.color, true) }">{{ item.color }}</div>
      </template>
      <template #date="{ item, col }">
        {{ item[col.field]?.toLocaleDateString('sv') }}
      </template>
    </data-table>
  </div>
</template>

<script setup lang="ts">
import { useDebounceFn, useLocalStorage } from '@vueuse/core'
import { watchEffect } from 'vue'
import { createPerson, invertColor, Person } from './example-data'
import { TableColumn, TableFilter } from '/Components/Tables/data-table'
import '/Components/Tables/data-table.vue'

const columns = ref([
  { field: 'name', hidden: false },
  { field: 'age' },
  { field: 'sex' },
  { field: 'color' },
  { field: 'date' }
])

const visibleColumns = ref<TableColumn[]>([])
watchEffect(() => {
  visibleColumns.value = columns.value.filter(x => !x.hidden)
})

const items = ref<Person[]>([])

const filter = ref<TableFilter>()
const filteredItems = ref<Person[]>([])
const filterString = ref('')

const filterDebounced = computed({
  get: () => filterString.value,
  set: useDebounceFn(value => filterString.value = value, 200)
})

watchEffect(() => {
  let result = items.value

  // Apply global search filter
  const searchTerm = filterString.value.toLowerCase()
  if (searchTerm) {
    result = result.filter(item => {
      return Object.values(item).some(value => {
        if (value == null) return false
        return String(value).toLowerCase().includes(searchTerm)
      })
    })
  }


  // Apply column-specific filter
  const colFilter = filter.value
  if (colFilter?.value) {
    const field = colFilter.column.field
    const filterValue = colFilter.value.toLowerCase()
    result = result.filter(item => {
      const value = item[field]
      if (value == null) return false
      return String(value).toLowerCase().includes(filterValue)
    })
  }

  filteredItems.value = result
})

const sortField = useLocalStorage('sortField', 'name')
const sortOrder = useLocalStorage('sortOrder', 1)

const selected = ref<Person[]>([])

const changeQuantity = useLocalStorage<number>('changeQuantity', 1)
const add = (quantity?: number) => {
  quantity ??= changeQuantity.value

  const max = items.value.length + quantity

  for (let i = items.value.length; i < max; i++) {
    items.value.push(createPerson())
  }
}
const remove = (quantity?: number) => {
  quantity ??= changeQuantity.value

  items.value.splice(0, quantity)
}

add(changeQuantity.value)

const contextMenuElement = ref<Components["ContextMenu"]>()

const onHeaderContextMenu = (column: TableColumn, event: MouseEvent) => {
  contextMenuElement.value?.show(event, [
    {
      name: 'Hide',
      icon: MdiFileHidden,
      command: () => column.hidden = true
    },
    {
      name: 'Restore',
      icon: MdiFileRestore,
      command: () => columns.value.forEach(x => x.hidden = false),
      visible: () => columns.value.some(x => x.hidden)
    }
  ])
}

const onRowContextMenu = (item: Person, column: TableColumn, event: MouseEvent) => {
  contextMenuElement.value?.show(event, [
    {
      name: 'Remove',
      icon: MdiFileRemove,
      command: () => {
        if (!selected.value.includes(item)) {
          items.value = items.value.filter(x => x != item)
        }
        else {
          items.value = items.value.filter(x => !selected.value.includes(x))
        }
      }
    }
  ])
}
</script>