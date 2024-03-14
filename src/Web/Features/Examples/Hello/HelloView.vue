<template>
  <div class="text-center col-md-6 col-lg-4">
    <p>
      {{ hello }}
      <MdiAccountCowboyHat />
    </p>
    <h1>
      <text-rotated :text="hello" :rotation>
        <MdiAccountCowboyHat />
      </text-rotated>
    </h1>
    <btn v-if="isActive" @click="pause()">
      Pause
    </btn>
    <btn v-else @click="resume()">
      Resume
    </btn>
    <range v-model.number="speed" min="-20" max="20" class="mt-3" />
    <progress-bar v-model="absoluteSpeed" max="20" class="mb-2" />
    Rotation speed
    <br />
    {{ speed }}
  </div>
</template>

<script setup>
import { rotation, speed, isActive, pause, resume, load } from './HelloData'

const { hello } = await load()

const absoluteSpeed = computed(() => Math.abs(speed.value))
</script>