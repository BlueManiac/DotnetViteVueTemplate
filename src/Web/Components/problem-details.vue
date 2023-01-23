<template>
  Component error
  <h5>{{file}}</h5>
  <template v-if="error.exception">
    Endpoint
    <h5>{{error.exception.endpoint}}</h5>
    <template v-if="Object.keys(error.exception.routeValues).length !== 0">
      Route values
      <h5>{{error.exception.routeValues}}</h5>
    </template>
    Status code
    <h5>{{error.status}}</h5>
    Exception
    <h5>{{error.exception.details}}</h5>
  </template>
  <template v-else>
    Error
    <h5>{{error?.stack ?? error}}</h5>
  </template>
</template>

<script setup>
  const { error, component } = defineProps({
    error: [Object, String, Error],
    component: Object
  })

  const file = component?.type?.__file

  console.error("Error occurred in " + file + "\n", error)
</script>

<style scoped>
  h5 {
    font-size: 1em;
    white-space: pre-line;
    word-wrap: anywhere;
  }
</style>