<template>
  <btn @click="send()" class="mb-3">Send</btn>
  <br />
  {{ message }}
</template>

<script setup>
import { api } from '/Features/api';

const { connection, data } = api.signalr('/api/realtime')

const message = data('message')

const send = () => connection.send('Data', 'hi')
</script>