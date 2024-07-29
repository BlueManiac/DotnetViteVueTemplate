<template>
  <div class="dropdown" :class="{ 'open': isOpen }">
    <div class="form-control" @click="isOpen = !isOpen">
      <span v-if="!selectedOptions.length">{{ placeholder }}</span>
      <span v-else>
        <span class="badge text-bg-secondary me-1" v-for="item in selectedOptions" :key="item">{{ item }}</span>
      </span>
      <div class="dropdown-toggle float-end"></div>
    </div>
    <div class="dropdown-menu w-100 p-0" :class="{ show: isOpen }" ref="menu">
      <div v-for="(option, index) in options" :key="index" class="form-check m-0 d-flex">
        <input class="form-check-input" type="checkbox" :id="'option-' + index" :value="option.value" v-model="selectedOptions" />
        <label class="form-check-label d-block" :for="'option-' + index">
          {{ option.label }}
        </label>
      </div>
    </div>
  </div>
</template>

<script lang="ts">
export interface Option {
  label: string
  value: any
}
</script>

<script setup lang="ts">
import { onClickOutside } from '@vueuse/core'
import { defineProps, ref, watch } from 'vue'

const { options } = defineProps<{
  placeholder: string,
  options: Option[],
}>()

const selectedOptions = defineModel<any[]>()
const isOpen = ref(false)

watch(selectedOptions, () => {
  selectedOptions.value = selectedOptions.value.sort((a, b) => {
    return options.findIndex(option => option.value === a) - options.findIndex(option => option.value === b)
  })
})


const menu = ref()

onClickOutside(menu, () => {
  isOpen.value = false
})
</script>

<style scoped>
.dropdown {
  &.open > .form-control {
    border-bottom: 0;
    border-bottom-left-radius: 0;
    border-bottom-right-radius: 0;
  }

  .dropdown-menu {
    border-top-left-radius: 0;
    border-top-right-radius: 0;

    > .form-check {
      padding-left: calc(.75rem + 1.5em);
      padding-right: calc(.75rem + 1.5em);

      &:hover {
        background-color: var(--bs-secondary);
      }
    }
  }
}
</style>
