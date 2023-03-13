<template>
  <div class="text-center col-4">
    <p>
      {{hello}}
      <IconMdiAccountCowboyHat />
    </p>
    <h1>
      <text-rotated :text="hello" :rotation="rotation">
        <IconMdiAccountCowboyHat />
      </text-rotated>
    </h1>
    <btn v-if="isActive" @click="pause()">
      Pause
    </btn>
    <btn v-else @click="resume()">
      Resume
    </btn>
    <range v-model.number="speed" min="-20" max="20" class="mt-3"></range>
    Rotation speed
    <br />
    {{speed}}
  </div>
</template>

<script setup>
  import { rotation, speed, isActive, pause, resume, load } from './HelloData'

  const { hello } = await load()
</script>