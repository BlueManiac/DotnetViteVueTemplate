import { onClickOutside, until } from '@vueuse/core'
import { Component, createVNode, nextTick, onUnmounted, reactive, toRef, unref, UnwrapRef, VNode } from 'vue'
import modal from "./modal.vue"

export {
  modal
}

export const modals = ref<VNode[]>([])

export class ModalState<TResult = unknown> {
  visible = false
  state: TResult

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

export const showModal = async <TResult = undefined>(component: Component, props = {}, state: TResult | null = undefined): Promise<UnwrapRef<TResult>> => {
  const modalState = reactive(new ModalState<TResult>())

  if (state !== undefined) {
    toRef(modalState, 'state').value = state
  }

  const node = createVNode(component, { ...props, modelValue: modalState })

  modals.value.push(node)

  await nextTick()

  modalState.show()

  await until(() => modalState.visible).toBe(false)

  modals.value = modals.value.filter(x => x !== node)

  return unref(modalState.state)
}