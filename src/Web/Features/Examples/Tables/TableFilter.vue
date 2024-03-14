<template>
    <div v-if="visible" class="position-absolute" :style="style" ref="root">
        <slot></slot>
    </div>
</template>

<script setup lang="ts">
import { onClickOutside } from '@vueuse/core';
import { StyleValue } from 'vue';

export type FilterModel = { top?: number, left?: number, visible?: boolean, width?: number }

const parentElement = defineModel<HTMLElement>("parent");

const visible = computed(() => !!parentElement.value)
const style = computed<StyleValue>(() => {
    const { bottom, left, width } = parentElement.value.getBoundingClientRect();

    return {
        top: bottom + "px",
        left: left + "px",
        minWidth: width + "px"
    }
})

const root = ref<HTMLElement>();
onClickOutside(root, () => parentElement.value = null)
</script>

<style scoped>
:host {
    background-color: red;
}
</style>