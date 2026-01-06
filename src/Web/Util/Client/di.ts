import {
  getCurrentInstance,
  createApp as vueCreateApp,
  inject as vueInject,
  type InjectionKey,
} from 'vue-original'

type Ctor<T> = new (...args: any[]) => T
const keyFor = <T>(ctor: Ctor<T>): InjectionKey<T> => Symbol.for(`AppDI:${ctor.name}`) as InjectionKey<T>

// Store factory functions for lazy instantiation
const factories = new Map<symbol, () => any>()

export function createApp(...args: Parameters<typeof vueCreateApp>) {
  const app = vueCreateApp(...args)

  const originalProvide = app.provide
  app.provide = function <T>(
    this: typeof app,
    arg1: InjectionKey<T> | string | Ctor<T> | T,
    arg2?: T | (() => T)
  ) {
    // Handle explicit class + instance/factory
    if (typeof arg1 === 'function' && arg2 !== undefined) {
      const ctor = arg1 as Ctor<T>
      const key = keyFor(ctor)

      // If arg2 is a factory function, store it for lazy instantiation
      if (typeof arg2 === 'function') {
        factories.set(key as symbol, arg2 as () => T)
        return this
      }

      // Otherwise provide the instance directly
      return originalProvide.call(this, key, arg2)
    }

    // Handle string/symbol keys
    if (arg2 !== undefined || typeof arg1 === 'string' || typeof arg1 === 'symbol') {
      return originalProvide.call(this, arg1 as any, arg2)
    }

    // Handle class constructor only (lazy instantiation)
    if (typeof arg1 === 'function') {
      const ctor = arg1 as Ctor<T>
      const key = keyFor(ctor)

      // Store factory function for lazy instantiation
      factories.set(key as symbol, () => new ctor())

      return this
    }

    // Handle instance only - infer constructor
    const instance = arg1 as T & { constructor: any }
    return originalProvide.call(this, keyFor(instance.constructor), instance)
  }

  return app
}

export * from 'vue-original'

export function inject<T>(
  keyOrCtor: InjectionKey<T> | string | Ctor<T>,
  defaultValue?: T
): T {
  if (typeof keyOrCtor === 'function') {
    const componentInstance = getCurrentInstance()

    if (!componentInstance) {
      throw new Error('inject() can\'t be called outside of a Vue app context.')
    }

    const key = keyFor(keyOrCtor)
    let instance = vueInject(key, defaultValue)

    if (instance) {
      return instance
    }

    // If not found and we have a factory, create the instance lazily
    const factory = factories.get(key as symbol)
    instance = factory?.()

    if (!instance) {
      throw new Error(`Dependency not found: ${keyOrCtor.name}. Make sure it's provided via app.provide().`)
    }

    // Provide it to the app so subsequent injects get the same instance
    const app = componentInstance.appContext.app
    app.provide(key, instance)

    // Remove factory since we've instantiated it
    factories.delete(key as symbol)

    return instance
  }

  const instance = vueInject(keyOrCtor as any, defaultValue)

  return instance as T
}

declare module 'vue' {
  interface App {
    provide<T>(instance: T): this
    provide<T>(ctor: Ctor<T>): this
    provide<T>(ctor: Ctor<T>, factory: () => T): this
    provide<T>(ctor: Ctor<T>, instance: T): this
    provide<T>(key: InjectionKey<T> | string, value: T): this
  }
  function inject<T>(ctor: Ctor<T>): T
}

