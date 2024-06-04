<template>
  <div>
    <div class="mb-4">
      <btn @click="open()">Open inline options modal</btn>
      <p>
        Modal state: {{ modalState }}<br />
        Modal header: {{ header }}
      </p>
      <inlineModal class="modal-sm">
        <template #title>
          {{ header }}
        </template>
        <p>
          Modal state: {{ modalState }}
        </p>
        <input type="text" class="form-control" v-model="header" />
        <template #footer>
          <btn @click="modalState.visible = false">Save</btn>
        </template>
      </inlineModal>
    </div>
    <div>
      <btn @click="confirm()">Open component confirm modal</btn>
      <p v-if="confirmResult">
        Modal component result: {{ confirmResult }}
      </p>
    </div>
  </div>
</template>

<script setup lang="ts">
import { showModal, useModal } from '../../../Components/Modals/modal'
import ConfirmModal from './ConfirmModal.vue'

const { modal: inlineModal, open, state: modalState } = useModal()
const header = ref('Options')

const confirmResult = ref()
const confirm = async () => {
  confirmResult.value = await showModal(ConfirmModal, { header: header.value })
}
</script>