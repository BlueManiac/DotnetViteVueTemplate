<template>
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
    Error
    <h5 class="text-danger">{{error?.message ?? error}}</h5>
    <template v-if="source">
      <div class="card col-lg-6 mb-2">
        <h5 class="card-header">{{source.fileShort}}</h5>
        <div class="card-body p-2 d-flex">
          <div class="pe-2">
            <pre v-for="line in lines" class="m-0">{{line.line}}<br /></pre>
          </div>
          <div>
            <pre v-for="line in lines" class="m-0" v-bind:class="{'text-bg-danger': line.error }">{{line.text}}<br /></pre>
          </div>
        </div>
      </div>
      Stack
      <pre>{{stack}}</pre>
    </template>
  </template>
</template>

<script setup lang="ts">
  import { watchEffect } from "vue"
  import StackTracey from 'stacktracey'
  import { Buffer } from 'buffer'

  // Needed for data-uri-to-buffer dependency in StackTracey
  window.Buffer = Buffer

  const { error } = defineProps<{
    error: (object | string | Error) & { problemDetails: any, stack: any }
  }>()

  const problemDetails = computed(() => error.problemDetails)

  const lines = ref([])
  const stack = ref()
  const source = ref()

  watchEffect(async () => {
    console.error("Error occurred %s", error.stack)

    if (!(error instanceof Error))
      return;

    const stackTrace = await new StackTracey(error as Error).cleanAsync();

    stack.value = stackTrace.asTable();

    source.value = stackTrace.withSourceAt(0)

    lines.value = source.value?.sourceFile?.lines
      .map((x, i) => ({ text: x, line: i + 1, error: i === source.value.line - 1 }))
      .slice(Math.max(0, source.value.line - 11), Math.min(source.value.sourceFile.lines.length, source.value.line + 9)) ?? [];
  })
</script>

<style scoped>
  h5 {
    font-size: 1em;
    white-space: pre-line;
    word-wrap: anywhere;
  }
</style>