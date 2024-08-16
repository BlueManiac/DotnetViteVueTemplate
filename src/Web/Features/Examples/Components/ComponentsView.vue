<template>
  <form class="col-lg-4 d-flex flex-column gap-2" v-validate>
    <checkbox v-model="checked">Checkbox</checkbox>
    <input-text required>Text</input-text>
    <input-datetime v-model="date" />
    <input-datetime v-model="date2" />
    <input-date v-model="date3" />
    <input-number v-model="numberValue" min="-3" max="5" />
    <radio v-model="radioValue" value="radio1">Radio 1</radio>
    <radio v-model="radioValue" value="radio2">Radio 2</radio>
    <div>
      <btn-file v-model="file">
        Choose file...
      </btn-file>
      {{ file?.name }}
      <btn v-if="file" @click="file = null">Clear</btn>
    </div>
    Selected Values: {{ dropdownValues }}
    <dropdown placeholder="Select Options" :options="dropdownOptions" v-model="dropdownValues" />
    <btn type="submit">Submit</btn>
  </form>
</template>

<script setup lang="ts">
import { ref } from 'vue'
import { Option } from '/Components/dropdown.vue'
import { vValidate } from '/Components/Validation/v-validate'

const checked = ref(false)
const date = ref(new Date())
const date2 = ref("2024-06-25T16:14:36.186Z")
const date3 = ref(new Date())
const numberValue = ref(null)
const radioValue = ref("radio1")
const file = ref<File>(null)

const dropdownOptions = ref<Option[]>([
  { label: 'Option 1', value: 'option1' },
  { label: 'Option 2', value: 'option2' },
  { label: 'Option 3', value: 'option3' }
])
const dropdownValues = ref([])
</script>