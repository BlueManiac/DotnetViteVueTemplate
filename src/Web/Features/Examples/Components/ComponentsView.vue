<template>
  <div class="col-lg-3 d-flex flex-column gap-2">
    <checkbox v-model="checked">Checkbox</checkbox>
    <input-datetime v-model="date" />
    <input-datetime v-model="date2" />
    <input-date v-model="date3" />
    <radio v-model="radioValue" value="radio1">Radio 1</radio>
    <radio v-model="radioValue" value="radio2">Radio 2</radio>
  </div>
</template>

<script setup>
const checked = ref(false)
const date = ref(new Date())
const date2 = ref("2024-06-25T16:14:36.186Z")
const date3 = ref(new Date())
const radioValue = ref("radio1")
</script>