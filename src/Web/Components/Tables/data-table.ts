import { watchArray } from '@vueuse/core';
import entryUnbind from 'core-js/internals/entry-unbind';
import { Ref, onBeforeUnmount, watch } from 'vue';

export const useVirtualization = () => {
  const showSet = ref(new Set<any>())
  const isLoaded = ref(false)

  const observer = new IntersectionObserver((entries) => {
    for (const entry of entries) {
      const elem = entry.target as HTMLTableRowElement & { __vnode: any };

      if (!elem.parentNode) {
        return;
      }

      const index = elem.__vnode.key

      if (entry.isIntersecting) {
        showSet.value.add(index);
      }
      else {
        showSet.value.delete(index);
      }
    }

    isLoaded.value = true
  }, { rootMargin: "500px 0px 500px 0px" });

  const setVisible = (element: HTMLTableRowElement) => {
    if (!element) {
      return;
    }

    observer.observe(element)
  }

  const isVisible = (index: any) => {
    return showSet.value.has(index)
  }

  onBeforeUnmount(() => {
    observer.disconnect()
  })

  return { setVisible, isVisible, isLoaded }
}

export const useSelection = (items: Ref<any[]>, selected: Ref<any[]>) => {
  const selectedSet = ref(new Set<number>())

  const toggleSelected = (item, checked) => {
    if (checked) {
      selectedSet.value.add(item)
    } else {
      selectedSet.value.delete(item)
    }
  }

  const selectAll = (checked) => {
    if (checked && selectedSet.value.size == 0) {
      for (const item of items.value) {
        selectedSet.value.add(item)
      }
    }
    else {
      selectedSet.value.clear()
    }
  }

  const checkbox = ref<HTMLInputElement>()

  watchArray(items, (newList, oldList, added, removed) => {
    for (const item of removed) {
      selectedSet.value.delete(item)
    }
  })

  watch(() => [selectedSet.value.size, items.value.length], () => {
    checkbox.value.checked = selectedSet.value.size > 0
    checkbox.value.indeterminate = selectedSet.value.size > 0 && selectedSet.value.size != items.value.length
  })

  watch(() => selectedSet.value.size, () => {
    selected.value = [...selectedSet.value]
  })

  return { selectedSet, toggleSelected, selectAll, checkbox }
}

export const useSorting = (sortField: Ref<string>, sortOrder: Ref<number>, columns: Ref<any[]>, items: Ref<any[]>) => {
  sortField.value ??= (columns.value.find(x => x.sort) ?? columns[0])?.field
  sortOrder.value ??= columns.value.find(x => x.sort)?.descending ? -1 : 1

  const sort = (column) => {
    if (sortField.value == column.field) {
      sortOrder.value = sortOrder.value * -1
    } else {
      sortField.value = column.field
      sortOrder.value = 1
    }
  }

  const sortItems = () => {
    const field = sortField.value
    const order = sortOrder.value

    if (order == 1) {
      items.value.sort((a, b) => {
        const field1 = a[field]
        const field2 = b[field]

        if (typeof field1 == "boolean")
          return Number(field1) - Number(field2)

        if (!field1)
          return -1

        if (!field2)
          return 1

        return Number.isInteger(field1)
          ? field1 - field2
          : field1.localeCompare(field2)
      })
    } else {
      items.value.sort((a, b) => {
        const field1 = a[field]
        const field2 = b[field]

        if (typeof field2 == "boolean")
          return Number(field2) - Number(field1)

        if (!field2)
          return -1

        if (!field1)
          return 1

        return Number.isInteger(field2)
          ? field2 - field1
          : field2.localeCompare(field1)
      })
    }
  }

  watch(() => items, () => {
    sortItems()
  }, { immediate: true, deep: true })

  watch(() => [sortField.value, sortOrder.value], ([field, order]) => sortItems())

  return { sort }
}

export const useClick = (selectedSet: Ref<Set<any>>, emit) => {
  const onRowClick = (data, column, event) => {
    // if the click was on the selection column, do nothing
    if (hasParentClass(event.target, 'selection-column')) {
      return;
    }

    // if the click was on a link, do nothing
    if (event.target != event.currentTarget) {
      return
    }

    // Support ctrl+click to select multiple rows
    if (event.ctrlKey) {
      if (selectedSet.value.has(data))
        selectedSet.value.delete(data)
      else
        selectedSet.value.add(data)

      return
    }

    emit('rowClick', data, column, event)

    function hasParentClass(element, className) {
      do {
        if (element.classList && element.classList.contains(className)) {
          return true;
        }
        element = element.parentNode;
      } while (element);
      return false;
    }
  }

  const onHeaderContextMenu = (column, event) => {
    if (event.ctrlKey) {
      return
    }
    event.preventDefault()
    emit('headerContextMenuClick', column, event)
  }

  const onRowContextMenu = (item, event) => {
    if (event.ctrlKey) {
      return
    }
    event.preventDefault()
    emit('rowContextMenuClick', item, event)
  }

  return { onRowClick, onHeaderContextMenu, onRowContextMenu }
}

const useDragDrop = (table: HTMLTableElement, visibleColumns: Ref<any[]>) => {
  // Drag and drop columns
  const onDragStart = (event, col) => {
    event.dataTransfer.setData('text', col.field)
  }

  const onDragOver = event => {
    event.preventDefault()

    const isLeft = event.offsetX < event.target.offsetWidth / 2

    if (isLeft) {
      event.currentTarget.classList.remove('drag-over-right')
      event.currentTarget.classList.add('drag-over-left')
    }
    else {
      event.currentTarget.classList.remove('drag-over-left')
      event.currentTarget.classList.add('drag-over-right')
    }
  }

  const onDragLeave = event => {
    event.currentTarget.classList.remove('drag-over-left')
    event.currentTarget.classList.remove('drag-over-right')
  }

  const onDrop = (event, col) => {
    let field = event.dataTransfer.getData('text')

    event.currentTarget.classList.remove('drag-over-left')
    event.currentTarget.classList.remove('drag-over-right')

    if (field == col.field)
      return

    const isLeft = event.offsetX < event.target.offsetWidth / 2

    const columns = [...visibleColumns.value]

    columns.splice(columns.indexOf(field), 1)

    const index = columns.indexOf(col.field)

    if (isLeft) {
      columns.splice(index, 0, field)
    }
    else {
      columns.splice(index + 1, 0, field)
    }

    visibleColumns.value = columns
  }

  //draggable @dragstart="onDragStart($event, col)" @dragenter.prevent @dragover="onDragOver" @dragleave="onDragLeave" @drop="onDrop($event, col)"
}