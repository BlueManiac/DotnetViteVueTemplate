<template>
  <btn @click="send()" class="mb-3">Send</btn>
  <br />
  {{message}}
</template>

<script setup>
  const { connection, data } = api.signalr('/realtime')

  const message = data('message')

  const send = () => connection.send('Data', 'hi')
</script>