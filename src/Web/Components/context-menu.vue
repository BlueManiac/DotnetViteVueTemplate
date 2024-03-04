<template>
  <ul class="dropdown-menu" ref="element" :style="style">
    <slot :value="value" :actions="menuActions" :event="clickEvent">
      <li v-for="item in menuActions">
        <a class="dropdown-item" href="#" @click="() => { item.command(); visible = false }">
          <component v-if="item.icon" :is="item.icon" />
          {{item.name}}
        </a>
      </li>
    </slot>
  </ul>
</template>

<script setup>
  import { onClickOutside } from '@vueuse/core'
  import { shallowRef } from 'vue'

  const value = defineModel('value')

  const element = ref()

  const visible = ref(false)
  const x = ref()
  const y = ref()

  const menuActions = shallowRef([])

  onClickOutside(element, () => {
    visible.value = false
    menuActions.value = []
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

  const clickEvent = ref()

  const show = (event, actions) => {
    x.value = event.pageX
    y.value = event.pageY
    menuActions.value = actions.filter(x => x.visible ? x.visible() : true)
    clickEvent.value = event
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