import modal from "./modal.vue"
import { h, reactive, render, VNode } from 'vue'
import { until } from '@vueuse/core'

// Check https://github.com/vuejs/router/blob/main/packages/router/e2e/modal/index.ts

export const useModal = (props: Object = {}, component: VNode = null) => {
  const state = reactive({
    visible: false
  })

  const open = async () => {
    state.visible = true
    await until(() => state.visible).toBe(false)
  }
  const close = () => {
    state.visible = false
  }

  return {
    modal: h(component ?? modal, { ...props, state, modal: state }),
    open,
    close,
    state,
    toggle: async () => {
      if (state.visible) {
        close()
      }
      else {
        await open()
      }
    }
  }
}

export const showModal = async (component, props) => {
  const response = {}
  const emits = {}

  if (component.emits) {
    for (const emit of component.emits) {
      const modelValue = emit.split(':')[1]

      emits['onUpdate:' + modelValue] = (value) => response[modelValue] = value
    }
  }

  const { modal, open } = useModal({ ...props, ...emits }, component)

  const wrapper = document.createElement('div')

  render(modal, wrapper)

  await open()

  render(null, wrapper)

  return response
}