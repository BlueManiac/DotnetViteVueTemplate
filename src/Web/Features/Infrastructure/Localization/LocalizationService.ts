import { shallowRef, type App, type InjectionKey } from 'vue'
import type { TranslationFunctions, Translations } from './localization-types'

import en from '../../../Localization/en'
import sv from '../../../Localization/sv'

// Single source of truth: add new locales here
export const dictionaries = { en, sv } as const

export type Locales = keyof typeof dictionaries

export class LocalizationService {
  static readonly token: InjectionKey<LocalizationService> = Symbol(LocalizationService.name)

  readonly availableLocales: Locales[] = Object.keys(dictionaries) as Locales[]
  readonly locale = shallowRef<Locales>('en')
  readonly tRef = shallowRef<TranslationFunctions>(
    this.createTranslationFunctions(dictionaries.en)
  )

  constructor() {
    const stored = localStorage.getItem('locale') as Locales | null
    const browserLang = navigator.language.split('-')[0] as Locales

    const locale = stored || (browserLang in dictionaries ? browserLang : 'en')
    this.setLocale(locale)
  }

  setLocale(newLocale: Locales) {
    if (!(newLocale in dictionaries)) {
      console.warn(`Locale '${newLocale}' not found, falling back to 'en'`)
      newLocale = 'en'
    }

    this.locale.value = newLocale
    this.tRef.value = this.createTranslationFunctions(dictionaries[newLocale])
    localStorage.setItem('locale', newLocale)
  }

  get t(): TranslationFunctions {
    return this.tRef.value
  }

  /**
   * Translate by string key (for dynamic/programmatic usage)
   * @example
   * localization.translate('navigation.home') // 'Home'
   * localization.translate('greeting', { name: 'John' }) // 'Hello, John!'
   */
  translate(key: string, args?: Record<string, any>): string {
    const keys = key.split('.')
    const value = this.getNestedValueByKey(dictionaries[this.locale.value], keys)
    return this.translateValue(value, args)
  }

  getLocaleName(locale: Locales): string {
    if (!(locale in dictionaries)) {
      console.warn(`Locale '${locale}' not found`)
      return locale
    }
    return dictionaries[locale].localeName
  }

  registerGlobalProperty(app: App, propertyName: string) {
    Object.defineProperty(app.config.globalProperties, propertyName, {
      get: () => this.tRef.value
    })
  }

  private getNestedValueByKey(obj: Record<string, any>, keys: string[]): any {
    if (keys.length === 0) return obj
    const [key, ...rest] = keys
    if (obj === undefined || obj === null) {
      console.warn(`Translation path not found: ${keys.join('.')}`)
      return undefined
    }
    return this.getNestedValueByKey(obj[key], rest)
  }

  private translateValue(translation: any, args?: Record<string, any>, path?: string[]): string {
    if (translation === undefined || translation === null) {
      const key = path?.join('.') || 'unknown'
      console.warn(`Missing translation: ${key}`)
      return `[${key}]`
    }
    if (typeof translation === 'function') {
      try {
        return translation(args)
      } catch (error) {
        const key = path?.join('.') || 'unknown'
        console.error(`Translation function error for ${key}:`, error)
        return `[${key}]`
      }
    }
    return translation
  }

  // Proxy chain enables $t.navigation.home() syntax:
  // - get trap: $t.navigation → returns new Proxy with path ['navigation']
  // - apply trap: .home() → lookups translation at path ['navigation', 'home']
  private createTranslationFunctions(translations: Translations): TranslationFunctions {
    const createFn = (path: string[]): any => {
      return new Proxy(() => { }, {
        get: (_target, prop: string) => {
          return createFn([...path, prop])
        },
        apply: (_target, _thisArg, args) => {
          const value = this.getNestedValueByKey(translations, path)
          return this.translateValue(value, args[0], path)
        }
      })
    }

    return createFn([]) as TranslationFunctions
  }
}
