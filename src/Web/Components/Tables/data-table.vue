<template>
  <table class="table table-striped table-sticky table-hover">
    <thead>
      <tr v-show="isLoaded">
        <th class="fs-4 lh-1">
          <input class="form-check-input mt-0" type="checkbox" @input="selectAll(($event.target as HTMLInputElement).checked)" ref="checkbox">
        </th>
        <template v-for="col of columns" :key="col.field">
          <th @click="sort(col)" @contextmenu="onHeaderContextMenu(col, $event)" :class="{'table-active': col.field == sortField }">
            <template v-if="col.field == sortField">
              <span class="text-primary-emphasis pe-1">
                {{toValue(col.header) ?? col.field}}
              </span>
              <MdiSortAscending v-if="sortOrder == 1" />
              <MdiSortDescending v-else />
            </template>
            <template v-else>
              {{toValue(col.header) ?? col.field}}
            </template>
          </th>
        </template>
      </tr>
    </thead>
    <TransitionGroup name="list" tag="tbody">
      <tr v-for="(item, index) of items" :ref="el => observeElement(el as HTMLTableRowElement, index)" @contextmenu="onRowContextMenu($event, item, index)" :key="item[dataKey]" :class="{ 'table-active': selectedSet.has(item) }">
        <template v-if="isVisible(index)">
          <td class="fs-4 lh-1 selection-column" @click="onRowClick(item, null, $event)">
            <input class="form-check-input mt-0" type="checkbox" :checked="selectedSet.has(item)" @input="toggleSelected(item, ($event.target as HTMLInputElement).checked)">
          </td>
          <template v-for="col of columns" :key="col.field">
            <td @click="onRowClick(item, col, $event)">
              <slot :name="col.field" :item="item" :col="col">
                {{item[col.field]}}
              </slot>
            </td>
          </template>
        </template>
      </tr>
    </TransitionGroup>
  </table>
</template>

<script setup lang="ts">
import { useClick, useSelection, useSorting, useVirtualization } from './data-table'
import { MaybeRefOrGetter, toValue, watch } from 'vue';

const { dataKey, rowHeight = '33px' } = defineProps<{
  dataKey: string,
  rowHeight?: string
}>()

const columns = defineModel<(any & { field: string, header?: MaybeRefOrGetter<string> })[]>("columns", { local: true })
const items = defineModel<any[]>("items")
const sortField = defineModel<string>("sortField", { local: true })
const sortOrder = defineModel<number>("sortOrder", { local: true })
const selected = defineModel<any[]>("selected", { local: true })

const emit = defineEmits<{
  rowClick: [item: any, column: any, event: Event],
  headerContextMenuClick: [column: any, event: Event],
  rowContextMenuClick: [item: any, column: any, event: Event]
}>()

// Retrive columns from items if not set
watch(() => [items.value.length, columns.value], () => {
  if (columns.value && columns.value.length > 0)
    return

  columns.value = items.value.length > 0
    ? Object.keys(items.value[0]).map(key => ({ field: key }))
    : []
}, { immediate: true })

const { observeElement, isVisible, isLoaded } = useVirtualization()
const { selectedSet, toggleSelected, selectAll, checkbox } = useSelection(items, selected)
const { sort } = useSorting(sortField, sortOrder, columns, items)
const { onRowClick, onHeaderContextMenu, onRowContextMenu } = useClick(selectedSet, emit)
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

.list-enter-active {
  transition: all 0.3s ease;
}

.list-enter-from,
.list-leave-to {
  opacity: 0;
  transform: translateY(10px);
}
</style>