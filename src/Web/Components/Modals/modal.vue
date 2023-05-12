<template>
  <teleport to="#app" v-if="state.visible">
    <div class="modal fade" tabindex="-1" ref="wrapper">
      <div class="modal-dialog">
        <div class="modal-content">
          <div class="modal-header">
            <slot name="header" v-bind="props">
              <slot name="title" v-bind="props">
                <h5 class="modal-title">Modal title</h5>
              </slot>
              <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
            </slot>
          </div>
          <div class="modal-body">
            <slot v-bind="props" />
          </div>
          <div class="modal-footer">
            <slot name="footer" v-bind="props">
              <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">Close</button>
              <button type="button" class="btn btn-primary">Save changes</button>
            </slot>
          </div>
        </div>
      </div>
    </div>
  </teleport>
</template>

<script setup lang="ts">
  import { useEventListener } from "@vueuse/core";
  import { Modal } from "bootstrap"
  import { watch } from "vue"

  const props = defineProps<{
    state: any,
    modal: any
  }>()

  const wrapper = ref();
  const component = ref();

  watch(() => wrapper.value, () => {
    if (wrapper.value) {
      component.value = new Modal(wrapper.value, {})
      component.value.show();
    }
  })

  watch(() => props.state.visible, () => {
    if (!props.state.visible) {
      component.value?.hide();
    }
  })

  useEventListener(wrapper, 'hidden.bs.modal', () => {
    props.state.visible = false;
  })
</script>