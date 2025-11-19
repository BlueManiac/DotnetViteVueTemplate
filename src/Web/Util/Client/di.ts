import {
  getCurrentInstance,
  createApp as vueCreateApp,
  inject as vueInject,
  type InjectionKey,
} from 'vue-original'

type Ctor<T> = new (...args: any[]) => T
const keyFor = <T>(ctor: Ctor<T>): InjectionKey<T> => Symbol.for(`AppDI:${ctor.name}`) as InjectionKey<T>

export function createApp(...args: Parameters<typeof vueCreateApp>) {
  const app = vueCreateApp(...args)

  const originalProvide = app.provide
  app.provide = function <T>(
    this: typeof app,
    arg1: InjectionKey<T> | string | Ctor<T> | T,
    arg2?: T
  ) {
    if (arg2 !== undefined || typeof arg1 === 'string' || typeof arg1 === 'symbol') {
      return originalProvide.call(this, arg1 as any, arg2)
    }
    if (typeof arg1 === 'function') {
      const ctor = arg1 as Ctor<T>
      const instance = new ctor()
      return originalProvide.call(this, keyFor(ctor), instance)
    }
    const instance = arg1 as T
    const ctor = instance.constructor as Ctor<T>
    return originalProvide.call(this, keyFor(ctor), instance)
  }

  return app
}

export * from 'vue-original'

export function inject<T>(
  keyOrCtor: InjectionKey<T> | string | Ctor<T>,
  defaultValue?: T
): T | undefined {
  if (!getCurrentInstance()) {
    throw new Error('inject() called outside of a Vue app context.')
  }
  if (typeof keyOrCtor === 'function') {
    return vueInject(keyFor(keyOrCtor), defaultValue)
  }
  return vueInject(keyOrCtor as any, defaultValue)
}

declare module 'vue' {
  interface App {
    provide<T>(instance: T): this
    provide<T>(ctor: Ctor<T>): this
  }
  function inject<T>(ctor: Ctor<T>): T | undefined
}

