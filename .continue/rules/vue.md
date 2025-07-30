---
description: Make sure vue file use script setup and typescript
alwaysApply: true
---

Vue files (.vue) should use vue 3 composition api script setup using typescript with bootstrap 5 styling.

* Use defineModel instead of props for passed model variables
  ex. const modelValue = defineModel<string>()
* defineProps should use typescript
  ex. const { name } = defineProps<{ name: string }>()
* vue files should follow the following tag order: template, script setup, style scoped
* Do not add custom styles for Bootstrap components unless it is absolutely necessary to achieve a specific visual requirement that cannot be addressed by Bootstrap's built-in classes.
  ex. If you're styling a button using "btn btn-primary", do not add custom CSS for colors, padding, or margins if Bootstrap's default styles suffice.