<template>
  Component error
  <h5>{{file}}</h5>
  <template v-if="error.url">
    Url
    <h5><a target="_blank" :href="error.url">{{error.url}}</a></h5>
  </template>
  <template v-if="error.method">
    Method
    <h5>{{error.method}}</h5>
  </template>
  <template v-if="error.status">
    Status code
    <h5>{{error.status}}</h5>
  </template>
  <template v-if="problemDetails">
    Title
    <h5>{{problemDetails.title}}</h5>
    <template v-if="problemDetails.exception">
      <template v-if="Object.keys(problemDetails.exception.routeValues).length !== 0">
        Route values
        <h5>{{problemDetails.exception.routeValues}}</h5>
      </template>
      <pre>{{problemDetails.exception.details}}</pre>
    </template>
  </template>
  <template v-else>
    <pre>{{error?.stack ?? error}}</pre>
  </template>
</template>

<script setup>
  import { computed, watch, watchEffect } from "vue"

  const props = defineProps({
    error: [Object, String, Error],
    component: Object
  })

  const file = computed(() => props.component?.type?.__file)
  const problemDetails = computed(() => props.error.problemDetails)

  watchEffect(() => {
    console.error()
    console.error("Error occurred in %s\n%s", file.value, props.error.stack)
  })

</script>

<style scoped>
  h5 {
    font-size: 1em;
    white-space: pre-line;
    word-wrap: anywhere;
  }
</style>