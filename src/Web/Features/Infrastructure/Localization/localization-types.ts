// Types automatically inferred from translation files
/* eslint-disable */

import type en from '../../../Localization/en'
export type { Locales } from './LocalizationService'

// Infer the base structure from en.ts
export type Translation = typeof en
export type Translations = typeof en

// Transform translation structure to function signatures
export type TranslationFunctions = MapToFunctions<Translation>

// Helper type: recursively transform values to functions
type MapToFunctions<T> = {
  [K in keyof T]: T[K] extends (...args: infer Args) => any
  ? (...args: Args) => string // Keep functions with their params
  : T[K] extends object
  ? MapToFunctions<T[K]> // Recurse into nested objects
  : () => string // Transform string to function
}
