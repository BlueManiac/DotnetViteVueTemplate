<template>
  <modal>
    <template #header>
      {{header}}
    </template>
    <p class="m-0">
      Modal state: {{modal}}
    </p>
    <template #footer>
      <btn @click="set(true)">Yes</btn>
      <btn @click="set(false)">No</btn>
    </template>
  </modal>
</template>

<script setup>
  const props = defineProps({
    header: String,
    modal: Object
  });
  const emit = defineEmits(['update:confirmed'])

  const set = (result) => {
    emit('update:confirmed', result)

    props.modal.visible = false
  }
</script>