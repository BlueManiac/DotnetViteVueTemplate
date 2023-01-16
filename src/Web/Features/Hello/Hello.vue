<template>
  <p>
    {{hello}}
    <i-mdi-account-cowboy-hat />
  </p>
  <h1>
    <text-rotated :text="hello" :rotation="rotation">
      <i-mdi-account-cowboy-hat />
    </text-rotated>
  </h1>
  <button-primary v-if="isActive" @click="pause()">Pause</button-primary>
  <button-primary v-else @click="resume()">Resume</button-primary>
  <range v-model.number="speed" min="-20" max="20" class="mt-3"></range>
  Rotation speeds
  <br />
  {{speed}}
</template>

<script setup>
  import { rotation, speed, isActive, pause, resume } from './data'

  const { hello } = await fetch('/hello')
</script>