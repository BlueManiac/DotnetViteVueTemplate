<template>
    <div v-if="visible" class="position-absolute bg-body border shadow-sm rounded" :style="style" ref="root">
        <div class="p-2">
            <input-text v-model="localFilterValue" placeholder="Filter..." @keydown.enter="applyFilter" :focus="!!column" class="mb-2" />
            <div class="d-flex gap-1">
                <btn @click="applyFilter" class="btn-sm">Apply</btn>
                <btn @click="clearFilter" class="btn-sm">Clear</btn>
            </div>
        </div>
    </div>
</template>

<script setup lang="ts">
import { onClickOutside } from '@vueuse/core'
import { StyleValue, watch } from 'vue'
import { TableColumn } from '/Components/Tables/data-table'

const parentElement = defineModel<HTMLElement>("parent")
const column = defineModel<TableColumn>("column")
const filterValue = defineModel<string>("filterValue")

const localFilterValue = ref("")

// Update local value when column changes
watch(column, (newCol) => {
    if (newCol) {
        localFilterValue.value = filterValue.value || ""
    }
}, { immediate: true })

const visible = computed(() => !!parentElement.value)
const style = computed<StyleValue>(() => {
    if (!parentElement.value) return {}

    const { bottom, left, width } = parentElement.value.getBoundingClientRect()

    return {
        top: bottom + "px",
        left: left + "px",
        minWidth: width + "px",
        zIndex: 1000
    }
})

const root = ref<HTMLElement>()
onClickOutside(root, () => {
    parentElement.value = undefined
})

const applyFilter = () => {
    filterValue.value = localFilterValue.value
    parentElement.value = undefined
}

const clearFilter = () => {
    localFilterValue.value = ""
    filterValue.value = ""
    parentElement.value = undefined
}
</script>

<style scoped></style>