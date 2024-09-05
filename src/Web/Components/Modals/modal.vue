<template>
  <dialog ref="el" class="modal-dialog" :class="{ [`modal-${size}`]: size }">
    <div class="modal-content">
      <div v-if="$slots.header" class="modal-header">
        <slot name="header" />
      </div>
      <div class="modal-body">
        <slot />
      </div>
      <div v-if="$slots.footer" class="modal-footer">
        <slot name="footer" :close />
      </div>
    </div>
  </dialog>
</template>

<script setup lang="ts">
import { onClickOutside } from '@vueuse/core'
import { ref, watch } from 'vue'
import { ModalState } from '/Components/Modals/modal'

const { modal = true, size = null } = defineProps<{
  modal?: boolean,
  size?: 'sm' | null | 'lg' | 'xl'
}>()

const modelValue = defineModel<ModalState>({ default: { visible: false } })

const el = ref<HTMLDialogElement>()

const show = () => {
  if (modal) {
    el.value.showModal()
  }
  else {
    el.value.show()
  }
  modelValue.value.visible = true
}

const close = () => {
  el.value.close()
  modelValue.value.visible = false
}

watch(() => modelValue.value?.visible, visible => {
  if (visible) {
    show()
  }
  else {
    close()
  }
})

onClickOutside(el, () => {
  close()
})

defineExpose({
  show,
  close
})
</script>

<style scoped>
dialog::backdrop {
  background-color: rgba(0, 0, 0, 0.5);
}
</style>