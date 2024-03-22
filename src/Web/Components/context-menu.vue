<template>
  <ul class="dropdown-menu" ref="element" :style>
    <slot :actions>
      <li v-for="item in actions">
        <a class="dropdown-item" href="#" @click="() => { item.command(); visible = false }">
          <component v-if="item.icon" :is="item.icon" />
          {{ item.name }}
        </a>
      </li>
    </slot>
  </ul>
</template>

<script setup lang="ts">
import { onClickOutside } from '@vueuse/core'
import { shallowRef } from 'vue'

export type ContextMenuAction = { name: String, icon: any, command: () => void, visible?: () => boolean }

const element = ref()
const visible = ref(false)
const x = ref<number>()
const y = ref<number>()
const actions = shallowRef<ContextMenuAction[]>([])

onClickOutside(element, () => {
  visible.value = false
  actions.value = []
})

const style = computed(() => {
  if (!visible.value) {
    return
  }

  return {
    display: 'block',
    left: x.value + 'px',
    top: y.value + 'px'
  }
})

const show = (event: MouseEvent, contextMenuActions: ContextMenuAction[]) => {
  x.value = event.pageX
  y.value = event.pageY
  actions.value = contextMenuActions.filter(x => x.visible ? x.visible() : true)
  visible.value = true
}

defineExpose({
  show
})
</script>

<style scoped>
.dropdown-menu {
  --bs-dropdown-padding-y: 0;
  --bs-dropdown-border-radius: 0;
}

.dropdown-item:hover {
  color: var(--bs-dropdown-link-active-color);
  background-color: var(--bs-dropdown-link-active-bg);
}
</style>