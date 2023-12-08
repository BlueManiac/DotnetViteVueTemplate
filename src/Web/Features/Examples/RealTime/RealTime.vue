<template>
  <btn @click="send()" class="mb-3 me-1">Send</btn>
  <btn @click="sender.Data('hi')" class="mb-3">Send using proxy</btn>
  <br />
  using data method<br />
  {{ message }}
  <br />
  <br />
  using proxy<br />
  {{ message2 }}
</template>

<script setup lang="ts">
import { Ref } from 'vue';
import { api } from '/Features/api';

type RealtimeSender = {
  Data(message: string): Promise<void>
}
type RealtimeReciever = {
  message: Ref<string>
}

const { connection, data, sender, receiver } = api.signalr<RealtimeSender, RealtimeReciever>('/api/realtime')

const message = data<string>('message')
const message2 = receiver.message;

const send = () => connection.send('Data', 'hi')
</script>