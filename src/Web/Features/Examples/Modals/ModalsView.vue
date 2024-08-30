<template>
  <div class="col-lg-2 d-flex flex-column gap-1">
    <btn @click="modalElement.show()">Modal using ref</btn>
    <modal ref="modalElement">
      <template #header>Header</template>
      <p>Dialog using ref</p>
      <template #footer="{ close }">
        <btn @click="close()">Close</btn>
      </template>
    </modal>

    <btn @click="modalDialog.show()">Modal using useModal()</btn>
    <modal v-model="modalDialog" size="sm">
      <template #header>Header</template>
      <p>Modal using useModal</p>
      <template #footer="{ close }">
        <btn @click="close()">Close</btn>
      </template>
    </modal>

    <btn @click="dialog.showModal()">Dialog using useDialog</btn>
    <dialog class="modal-dialog modal-sm" ref="dialog">
      <div class="modal-content">
        <div class="modal-header">
          Header
        </div>
        <div class="modal-body">
          <p>Custom dialog</p>
        </div>
        <div class="modal-footer">
          <btn @click="dialog.close()">Close</btn>
        </div>
      </div>
    </dialog>

    <btn @click="customModal.show()">Custom modal component using useModal()</btn>
    <CustomModal v-model="customModal" size="lg" />

    <btn @click="customModal2.show()">Custom modal component with useModal(CustomModal)</btn>

    <btn @click="show()">Custom modal component with showModal(CustomModal)</btn>
  </div>
</template>

<script setup lang="ts">
import { ref } from 'vue'
import CustomModal from './CustomModal.vue'
import { modal, showModal, useDialog, useModal } from '/Components/Modals/modal'

const modalElement = ref<HTMLDialogElement>()
const modalDialog = useModal()
const dialog = useDialog()
const customModal = useModal()
const customModal2 = useModal(CustomModal)

const show = async () => {
  await showModal(CustomModal)
}
</script>