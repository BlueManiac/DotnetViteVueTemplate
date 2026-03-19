import { ref } from 'vue'

export function useFormValidation() {
  const valid = ref(false)

  const vValidate = {
    mounted(el: HTMLFormElement) {
      el.setAttribute('novalidate', 'true')

      const updateValidity = () => {
        valid.value = el.checkValidity()
      }

      el.addEventListener('input', updateValidity)

      el.addEventListener('submit', (event) => {
        event.preventDefault()

        if (!el.checkValidity()) {
          event.stopPropagation()
          el.classList.add('was-validated')
          return
        }

        el.classList.add('was-validated')
      })
    },
  }

  return { vValidate, valid }
}