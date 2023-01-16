<template>
    <div>
        <input type="range" class="form-range" :value="modelValue" @input="update" :min="min" :max="max">
    </div>
</template>

<script setup>
    defineProps({
        modelValue: Number,
        min: [Number, String],
        max: [Number, String]
    })

    const emit = defineEmits(['update:modelValue'])

    function update(event) {
        emit('update:modelValue', event.target.value)
    }
</script>