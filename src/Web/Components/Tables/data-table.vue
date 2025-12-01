<template>
  <table ref="tableRef" v-bind="$attrs" class="table table-striped table-sticky table-hover">
    <thead>
      <tr v-show="isLoaded">
        <th class="fs-4 lh-1">
          <input class="form-check-input mt-0" type="checkbox" @input="selectAll(($event.target as HTMLInputElement).checked)" ref="checkbox">
        </th>
        <template v-for="col of columns" :key="col.field">
          <th @click="sort(col)" @contextmenu="onHeaderContextMenu(col, $event)" :class="{ 'table-active': col.field == sortField }">
            <div class="d-flex align-items-center">
              <template v-if="col.field == sortField">
                <span class="text-primary-emphasis pe-1">
                  {{ toValue(col.header) ?? col.field }}
                </span>
                <MdiSortAscending v-if="sortOrder == 1" />
                <MdiSortDescending v-else />
              </template>
              <template v-else>
                {{ toValue(col.header) ?? col.field }}
              </template>
              <div v-if="filterable && col.filterable !== false" class="ms-auto" @click.stop="onFilterClick(col, $event, ($event.target as HTMLElement).closest('th')!)">
                <MdiFilter v-if="activeFilterColumn?.field === col.field" class="text-primary" />
                <MdiFilterOutline v-else />
              </div>
            </div>
          </th>
        </template>
      </tr>
    </thead>
    <tbody>
      <template v-for="(item, index) of filteredItems" :key="index" v-memo="memo(item, index)">
        <tr @contextmenu="onRowContextMenu($event, item, index)" :class="{ 'table-active': selectedSet.has(item), 'invisible': !isLoaded && !visibleIndexSet.has(index) }">
          <template v-if="visibleIndexSet.has(index)">
            <td class="fs-4 lh-1 selection-column" @click="onRowClick(item, undefined, $event)">
              <input class="form-check-input mt-0" type="checkbox" :checked="selectedSet.has(item)" @input="toggleSelected(item, ($event.target as HTMLInputElement).checked)">
            </td>
            <td v-for="col of columns" :key="col.field" @click="onRowClick(item, col, $event)">
              <slot :name="col.field" :item :col>
                {{ item[col.field] }}
              </slot>
            </td>
          </template>
        </tr>
      </template>
    </tbody>
  </table>
  <TableFilter v-if="filterable" v-model:parent="filterParent" v-model:filter="filter" />
</template>

<script setup lang="ts" generic="T extends Record<string, any>">
import { toValue, watch } from 'vue'
import { NamedTableColumn, TableColumn, TableFilter as TableFilterType, useClick, useFiltering, useSelection, useSorting, useVirtualization } from './data-table'
import TableFilter from './table-filter.vue'

defineOptions({
  inheritAttrs: false
})

const { rowHeight = '33px', filterable = true } = defineProps<{
  rowHeight?: string,
  filterable?: boolean
}>()

defineSlots<{
  [K in keyof T]?: (props: { item: T, col: NamedTableColumn<K> }) => any
}>()

const columns = defineModel<TableColumn[]>("columns", { default: () => [] })
const items = defineModel<T[]>("modelValue", { default: () => [] })
const sortField = defineModel<string>("sortField", { default: '' })
const sortOrder = defineModel<number>("sortOrder", { default: 1 })
const selectedModel = defineModel<T[]>("selected", { default: () => [] })
const filter = defineModel<TableFilterType>("filter")
const filteredItemsModel = defineModel<T[]>("filteredItems")

const emit = defineEmits<{
  rowClick: [item: T, column: TableColumn, event: MouseEvent],
  headerContextMenuClick: [column: TableColumn, event: MouseEvent],
  rowContextMenuClick: [item: T, column: TableColumn, event: MouseEvent]
}>()

// Retrive columns from items if not set
watch(() => [items.value?.length, columns.value], () => {
  if (columns.value && columns.value.length > 0)
    return

  columns.value = items.value?.length > 0
    ? Object.keys(items.value[0]).map(key => ({ field: key }))
    : []
}, { immediate: true })

const tableRef = ref<HTMLTableElement>()
const filteredItems = useFiltering(items, filter, columns)
const itemsCount = computed(() => filteredItems.value.length)

const { visibleIndexSet, isLoaded } = useVirtualization(tableRef, itemsCount)
const { selectedSet, toggleSelected, selectAll, checkbox } = useSelection(filteredItems)
const { sort } = useSorting(sortField, sortOrder, columns, filteredItems)
const { onRowClick, onHeaderContextMenu, onRowContextMenu } = useClick(selectedSet, emit)

// Sync selectedSet to model (observing the Set directly)
watch(selectedSet, () => {
  selectedModel.value = [...selectedSet.value]
}, { immediate: true })

// Sync filtered items to model
watch(filteredItems, (value) => {
  filteredItemsModel.value = value
}, { immediate: true })

// Filter functionality
const filterParent = ref<HTMLElement>()
const activeFilterColumn = computed(() => filter.value?.value ? filter.value.column : null)

const onFilterClick = (col: TableColumn, event: MouseEvent, headerElement: HTMLElement) => {
  if (!filter.value?.column || filter.value.column.field !== col.field) {
    filter.value = { ...filter.value, column: col, value: filter.value?.value || '' }
  }
  filterParent.value = headerElement
}

// Detect HMR changes for memoization
let hmrChange = 0
import.meta.hot?.on('vite:beforeUpdate', () => {
  hmrChange++
})

const memo = (item: T, index: number) => {
  const visible = visibleIndexSet.value.has(index)

  return [visible, visible && item, selectedSet.value.has(item), columns.value, visible && hmrChange]
}
</script>

<style scoped>
th {
  position: relative;

  &:hover {
    cursor: pointer;
  }
}

tr {
  height: v-bind(rowHeight);
}

th,
td {
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
  max-width: 50ch;
}
</style>