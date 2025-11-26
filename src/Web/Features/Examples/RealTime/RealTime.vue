<template>
  <div>
    <btn @click="send()" class="mb-3 me-1">Send</btn>
    <btn @click="sender.Test('hi')" class="mb-3">Send using proxy</btn>
    <br />
    using data method<br />
    {{ message }}
    <br />
    <br />
    using proxy<br />
    {{ message2 }}
  </div>
</template>

<script setup lang="ts">
import { inject, Ref } from 'vue'
import { ApiService } from '../../ApiService'

const api = inject(ApiService)

type RealtimeSender = {
  Test(message: string): Promise<void>
}
type RealtimeReciever = {
  message: Ref<string>
}

const { connection, data, sender, receiver } = api.useSignalr<RealtimeSender, RealtimeReciever>('/api/realtime')

const message = data<string>('message')
const message2 = receiver.message

const send = () => connection.send('Test', 'hi')
</script>