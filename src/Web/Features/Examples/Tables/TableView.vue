<template>
  <div class="d-flex gap-1 align-items-center mb-2">
    <btn @click="remove()">Remove</btn>
    <btn @click="add(1000)">Add 1000</btn>
    <span>Quantity: {{items.length}}</span>
  </div>
  Selected: {{selected.length}} {{selected[0]}}
  <context-menu ref="contextMenuElement" />
  <data-table class="table-sm" v-model="items" :columns="visibleColumns" v-model:selected="selected" v-model:sortField="sortField" v-model:sortOrder="sortOrder" dataKey="id" @headerContextMenuClick="onHeaderContextMenu" @rowContextMenuClick="onRowContextMenu">
    <template #id="{ item, col }">
      {{item[col.field]}}
    </template>
    <template #color="{ item }">
      <div class="px-2" :style="{ 'background-color': item.color, 'color': invertColor(item.color, true) }">{{item.color}}</div>
    </template>
    <template #date="{ item, col }">
      {{item[col.field]?.toLocaleDateString('sv')}}
    </template>
  </data-table>
</template>

<script setup lang="ts">
  import { useLocalStorage } from '@vueuse/core';
  import { createPerson, invertColor } from './example-data';

  const columns = ref([
    { field: 'name', hidden: false },
    { field: 'age' },
    { field: 'sex' },
    { field: 'color' },
    { field: 'date' }
  ]);
  const visibleColumns = computed(() => columns.value.filter(x => !x.hidden))
  const items = ref([]);

  const sortField = useLocalStorage('sortField', 'name')
  const sortOrder = useLocalStorage('sortOrder', 1)

  const selected = ref([])

  const remove = () => {
    items.value.splice(0, 1);
  }
  const add = quantity => {
    const max = items.value.length + quantity

    for (let i = items.value.length; i < max; i++) {
      items.value.push(createPerson());
    }
  }

  add(1000)

  const contextMenuElement = ref()

  const onHeaderContextMenu = (col, event) => {
    contextMenuElement.value.show(event, [
      { name: 'Hide', icon: MdiFileHidden, command: () => col.hidden = true },
      { name: 'Restore', icon: MdiFileRestore, command: () => columns.value.forEach(x => x.hidden = false), visible: () => columns.value.some(x => x.hidden) }
    ])
  }

  const onRowContextMenu = (event, item, index) => {
    contextMenuElement.value.show(event, [
      { name: 'Remove', command: () => items.value.splice(index, 1) }
    ])
  }
</script>