import { AsyncComputedOnCancel, AsyncComputedOptions, computedAsync, createEventHook, extendRef } from "@vueuse/core"
import { Ref, watch } from "vue"

export function computedWithTrigger<T>(evaluationCallback: (onCancel?: AsyncComputedOnCancel) => T | Promise<T>, initialState?: T, optionsOrRef?: Ref<boolean> | AsyncComputedOptions) {
  const result = ref<T>()
  const computedResult = computedAsync(evaluationCallback, initialState, optionsOrRef)

  watch(computedResult, value => {
    return result.value = value
  })

  const { on, trigger } = createEventHook()

  on(async () => {
    result.value = await evaluationCallback()
  })

  return extendRef(result, {
    trigger
  })
}