  <template>
    <div>
      <label v-if="slots.default" class="form-label" :for="id">
        <slot />
      </label>
      <div class="input-time">
        <input :type v-model="modelValue" class="form-control" :placeholder :disabled :id :required />
      </div>
    </div>
  </template>

<script setup lang="ts">
import { useSlots } from 'vue'

const {
  type = "time",
  placeholder,
  id = Math.random().toString(36).substring(2, 12),
  required,
  disabled
} = defineProps<{
  type?: string,
  placeholder?: string,
  id?: string,
  required?: boolean,
  disabled?: boolean
}>()
const modelValue = defineModel<string | Date>()
const slots = useSlots()
</script>

<style scoped>
.input-time {
  display: inline-block;
  width: 100%;
}
</style>
