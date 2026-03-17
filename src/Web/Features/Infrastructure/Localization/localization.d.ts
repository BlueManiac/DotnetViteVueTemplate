// Global type augmentation for Vue - adds $t to all components
import type { TranslationFunctions } from './localization-types'

declare module 'vue' {
  interface ComponentCustomProperties {
    $t: TranslationFunctions
  }
}
