<template>
  <button-primary @click="send()" class="mb-3">Send</button-primary>
  <br />
  {{message}}
</template>

<script setup>
  import { useSignalr } from "../../Util/Client/signalr"

  const { connection, data } = useSignalr(apiUrl + '/realtime')

  const message = data('message')

  const send = () => connection.send('Data', 'hi')
</script>