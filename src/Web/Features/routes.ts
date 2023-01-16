import { RouteRecordRaw } from 'vue-router'

export const baseRoutes: RouteRecordRaw[] = [
  {
    path: '/',
    component: () => import('./Hello/Hello.vue'),
    meta: { title: 'Home' }
  },
  {
    path: '/test',
    children: [{
      path: 'hello',
      component: () => import('./Hello/Hello.vue'),
      meta: { title: 'Hello' }
    }],
    meta: { title: 'Test' }
  },
]