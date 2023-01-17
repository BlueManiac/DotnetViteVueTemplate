import { RouteRecordRaw } from 'vue-router'

export const baseRoutes: RouteRecordRaw[] = [
  {
    path: '/',
    component: () => import('./Hello/Hello.vue'),
    meta: { title: 'Home' }
  },
  {
    path: '/test',
    meta: { title: 'Test' },
    children: [{
      path: 'hello',
      component: () => import('./Hello/Hello.vue'),
      meta: { title: 'Hello' }
    }]
  },
  {
    path: "/errors",
    meta: { title: 'Errors' },
    children: [
      {
        path: 'serverError',
        component: () => import('./Errors/ServerError.vue'),
        meta: { title: 'Server exception' }
      },
      {
        path: 'clientError',
        component: () => import('./Errors/ClientError.vue'),
        meta: { title: 'Client error' }
      }
    ]
  },
]