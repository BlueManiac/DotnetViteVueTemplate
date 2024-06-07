<template>
  <table class="table table-striped table-sticky table-hover">
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
              <div v-if="onFilterClick" class="ms-auto" @click.stop="emit('filterClick', col, $event, ($event.target as HTMLElement).closest('th'))">
                <MdiFilterOutline />
              </div>
            </div>
          </th>
        </template>
      </tr>
    </thead>
    <tbody>
      <template v-for="(item, index) of items" :key="index" v-memo="memo(item, index)">
        <tr :ref="el => observeElement(el as HTMLTableRowElement)" @contextmenu="onRowContextMenu($event, item, index)" :class="{ 'table-active': selectedSet.has(item) }">
          <template v-if="visibleIndexSet.has(index)">
            <td class="fs-4 lh-1 selection-column" @click="onRowClick(item, null, $event)">
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
</template>

<script setup lang="ts">
import { Column, useClick, useSelection, useSorting, useVirtualization } from './data-table'
import { toValue, watch } from 'vue'

const { rowHeight = '33px', onFilterClick } = defineProps<{
  rowHeight?: string,
  onFilterClick?: Function
}>()

const columns = defineModel<Column[]>("columns")
const items = defineModel<any[]>("modelValue")
const sortField = defineModel<string>("sortField")
const sortOrder = defineModel<number>("sortOrder")
const selected = defineModel<any[]>("selected")

const emit = defineEmits<{
  rowClick: [item: any, column: Column, event: MouseEvent],
  headerContextMenuClick: [column: Column, event: Event],
  rowContextMenuClick: [item: any, column: Column, event: Event],
  filterClick: [column: Column, event: MouseEvent, headerElement: HTMLElement]
}>()

// Retrive columns from items if not set
watch(() => [items.value.length, columns.value], () => {
  if (columns.value && columns.value.length > 0)
    return

  columns.value = items.value.length > 0
    ? Object.keys(items.value[0]).map(key => ({ field: key }))
    : []
}, { immediate: true })

const { observeElement, visibleIndexSet, isLoaded } = useVirtualization()
const { selectedSet, toggleSelected, selectAll, checkbox } = useSelection(items, selected)
const { sort } = useSorting(sortField, sortOrder, columns, items)
const { onRowClick, onHeaderContextMenu, onRowContextMenu } = useClick(selectedSet, emit)

// Detect HMR changes for memoization
let hmrChange = 0
import.meta.hot?.on('vite:beforeUpdate', () => {
  hmrChange++
})

const memo = (item: unknown, index: number) => {
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