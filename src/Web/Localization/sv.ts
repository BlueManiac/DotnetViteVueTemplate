import type { Translation } from '../Features/Localization/localization-types'

// Keep keys sorted alphabetically for maintainability
const sv = {
  localeName: 'Svenska',
  cancel: 'Avbryt',
  delete: 'Ta bort',
  edit: 'Redigera',
  error: 'Ett fel uppstod',
  language: 'Språk',
  loading: 'Laddar...',
  navigation: {
    examples: 'Exempel',
    home: 'Hem',
    translations: 'Översättningar'
  },
  save: 'Spara',
  success: 'Framgång!',
  welcome: 'Välkommen'
} satisfies Translation

export default sv
