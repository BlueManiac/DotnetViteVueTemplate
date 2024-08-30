import { onClickOutside, until } from '@vueuse/core'
import { Component, createVNode, nextTick, onUnmounted, reactive, VNode } from 'vue'
import modal from "./modal.vue"

export {
  modal
}

export const modals = ref<VNode[]>([])

export class ModalState {
  visible = false

  show() {
    this.visible = true
  }

  close() {
    this.visible = false
  }
}

export const useDialog = () => {
  const el = ref<HTMLDialogElement>()

  onClickOutside(el, () => {
    el.value?.close()
  })

  return el
}

export const useModal = (component: Component = null, props = {}) => {
  const state = reactive(new ModalState())

  if (component) {
    const node = createVNode(component, { ...props, modelValue: state })

    modals.value.push(node)

    onUnmounted(() => {
      modals.value = modals.value.filter(x => x !== node)
    })
  }

  return state
}

export const showModal = async (component: Component, props = {}) => {
  const state = reactive(new ModalState())

  const node = createVNode(component, { ...props, modelValue: state })

  modals.value.push(node)

  await nextTick()

  state.show()

  await until(() => state.visible).toBe(false)
}