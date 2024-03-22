<template>
  <modal>
    <template #header>
      {{ header }}
    </template>
    <p class="m-0">
      Modal state: {{ modal }}
    </p>
    <template #footer>
      <btn @click="set(true)">Yes</btn>
      <btn @click="set(false)">No</btn>
    </template>
  </modal>
</template>

<script setup lang="ts">
const { modal } = defineProps<{
  header: string,
  modal: any
}>();
const confirmed = defineModel<boolean>('confirmed')

const set = (result: boolean) => {
  confirmed.value = result;

  modal.visible = false
}
</script>