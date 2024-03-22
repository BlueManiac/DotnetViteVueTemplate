// Ensure vue file imports are not reported as warnings
declare module '*.vue' {
  import { DefineComponent } from 'vue'
  const component: DefineComponent<{}, {}, any>
  export default component
}

import { GlobalComponents } from 'vue';

// Expose all component instance types
declare global {
  type Components = {
    [Property in keyof GlobalComponents]: InstanceType<GlobalComponents[Property]>
  }
}