<template>
  <div>
    <div class="d-flex gap-2 align-items-center">
      <btn @click="add()">Add</btn>
      <btn @click="remove()">Remove</btn>
      <label>Quantity:</label>
      <input-number v-model="changeQuantity"></input-number>
      <input-text v-model="filterDebounced" placeholder="Filter" class="ms-auto" />
      <btn @click="filter = ''">Clear</btn>
    </div>
    <range v-model.number="changeQuantity" min="1" max="10000" class="mt-3" />
    Quantity: {{ items.length }}, Selected: {{ selected.length }} {{ selected[0] }}, Filtered: {{ filteredItems.length }}
    <context-menu ref="contextMenuElement" />
    <TableFilter v-model:parent="filterParent">
      <div class="p-1 bg-success">{{ filterData }}</div>
    </TableFilter>
    <data-table class="table-sm" v-model="filteredItems" :columns="visibleColumns" v-model:selected="selected" v-model:sortField="sortField" v-model:sortOrder="sortOrder" @headerContextMenuClick="onHeaderContextMenu" @rowContextMenuClick="onRowContextMenu" @filterClick="onFilterClick">
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
import TableFilter from './TableFilter.vue'
import { createPerson, invertColor } from './example-data'
import { Column } from '/Components/Tables/data-table'
import '/Components/Tables/data-table.vue'

const filter = ref('')
const filterDebounced = computed({
  get: () => filter.value,
  set: useDebounceFn(value => filter.value = value, 200)
})

const columns = ref([
  { field: 'name', hidden: false },
  { field: 'age' },
  { field: 'sex' },
  { field: 'color' },
  { field: 'date' }
])

const visibleColumns = ref([])
watchEffect(() => {
  visibleColumns.value = columns.value.filter(x => !x.hidden)
})

const items = ref([])

const filteredItems = ref([])
watchEffect(() => {
  filteredItems.value = items.value.filter(x => x.name.toLowerCase().includes(filter.value.toLowerCase()))
})

const sortField = useLocalStorage('sortField', 'name')
const sortOrder = useLocalStorage('sortOrder', 1)

const selected = ref([])

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

const onHeaderContextMenu = (col, event) => {
  contextMenuElement.value.show(event, [
    {
      name: 'Hide',
      icon: MdiFileHidden,
      command: () => col.hidden = true
    },
    {
      name: 'Restore',
      icon: MdiFileRestore,
      command: () => columns.value.forEach(x => x.hidden = false),
      visible: () => columns.value.some(x => x.hidden)
    }
  ])
}

const onRowContextMenu = (event, item, index) => {
  contextMenuElement.value.show(event, [
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

const filterParent = ref<HTMLElement>()
const filterData = ref(null)

const onFilterClick = (col: Column, event: MouseEvent, columnElement: HTMLElement) => {
  filterParent.value = columnElement
  filterData.value = col
}
</script>