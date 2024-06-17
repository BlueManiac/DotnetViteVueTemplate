<template>
  <div>
    <h1>
      Load icons using unplugin-vue-components:
      <IconMdiArrowDown />
    </h1>
    <h1>
      Or unplugin-auto-import:
      <component v-for="icon in icons" :is="icon" />
    </h1>
  </div>
</template>

<script setup>
const icons = [
  IconMdiAlarm,
  IconMdiAccountCowboyHat
]
</script>