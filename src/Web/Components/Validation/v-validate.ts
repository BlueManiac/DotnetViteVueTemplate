export const vValidate = {
  mounted(el: HTMLFormElement) {
    // Disable default validation tooltips
    el.setAttribute('novalidate', 'true')

    el.addEventListener('submit', (event) => {
      if (!el.checkValidity()) {
        event.preventDefault()
        event.stopPropagation()
      }

      el.classList.add('was-validated')
    })
  },
}