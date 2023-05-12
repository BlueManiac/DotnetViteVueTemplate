import { RouteRecordRaw } from 'vue-router'

export const baseRoutes: RouteRecordRaw[] = [
  {
    path: '/',
    component: () => import('./Home/HomeView.vue'),
    meta: { title: 'Home' }
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
  {
    path: '/realtime',
    component: () => import('./RealTime/RealTime.vue'),
    meta: { title: 'Realtime' }
  },
  {
    path: '/icons',
    component: () => import('./Icons/IconView.vue'),
    meta: { title: 'Icons' }
  },
  {
    path: "/modals",
    component: () => import('./Modals/ModalsView.vue'),
    meta: { title: 'Modals' }
  },
]