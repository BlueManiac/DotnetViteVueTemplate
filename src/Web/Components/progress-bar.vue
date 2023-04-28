<template>
  <div class="progress">
    <div class="progress-bar" role="progressbar" v-bind:aria-valuenow="modelValue" v-bind:aria-valuemin="min" v-bind:aria-valuemax="max"></div>
  </div>
</template>

<script setup>
  const props = defineProps({
    modelValue: Number,
    min: {
      type: [Number, String],
      default: 0
    },
    max: {
      type: [Number, String],
      default: 100
    },
  })

  const width = computed(() => props.modelValue/props.max * 100)
</script>

<style scoped>
  .progress-bar {
    width: calc(v-bind(width) * 1%);
  }
</style>