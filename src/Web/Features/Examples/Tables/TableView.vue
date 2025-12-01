<template>
  <div>
    <div class="d-flex gap-2 align-items-center">
      <btn @click="add()">Add</btn>
      <btn @click="remove()">Remove</btn>
      <label>Quantity:</label>
      <input-number v-model="changeQuantity"></input-number>
      <input-text v-model="filterDebounced" placeholder="Filter" class="ms-auto" />
      <btn @click="filter = {}">Clear</btn>
    </div>
    <range v-model.number="changeQuantity" min="1" max="10000" class="mt-3" />
    Quantity: {{ items.length }}, Selected: {{ selected.length }} {{ selected[0] }}, Filtered: {{ filteredItems.length }}
    <context-menu ref="contextMenuElement" />
    <data-table class="table-sm" v-model="items" v-model:filteredItems="filteredItems" :columns="visibleColumns" v-model:selected="selected" v-model:sortField="sortField" v-model:sortOrder="sortOrder" v-model:filter="filter" @headerContextMenuClick="onHeaderContextMenu" @rowContextMenuClick="onRowContextMenu">
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
import { createPerson, invertColor, Person } from './example-data'
import { TableColumn, TableFilter } from '/Components/Tables/data-table'
import '/Components/Tables/data-table.vue'

const columns = ref<TableColumn[]>([
  { field: 'name', hidden: false },
  { field: 'age' },
  { field: 'sex' },
  { field: 'color', filterable: false },
  { field: 'date' }
])

const visibleColumns = computed(() => columns.value.filter(x => !x.hidden))

const items = ref<Person[]>([])
const filteredItems = ref<Person[]>([])

const filter = ref<TableFilter>({})

const filterDebounced = computed({
  get: () => filter.value?.global || '',
  set: useDebounceFn(value => {
    if (!filter.value) filter.value = {}
    filter.value.global = value
  }, 200)
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