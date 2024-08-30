<template>
  <modal>
    <template #header>
      Custom header
    </template>
    <p class="m-0">
      Modal state: {{ state }}
      <slot />
    </p>
    <template #footer="{ close }">
      <btn @click="set(true)">Yes</btn>
      <btn @click="set(false)">No</btn>
      <btn @click="close()">Cancel</btn>
    </template>
  </modal>
</template>

<script setup lang="ts">
const state = ref(false)

const set = (result: boolean) => {
  state.value = result
}
</script>