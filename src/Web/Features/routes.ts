import { RouteRecordRaw } from 'vue-router'

export const baseRoutes: RouteRecordRaw[] = [
  {
    path: '/',
    component: () => import('./Examples/Home/HomeView.vue'),
    meta: { title: 'Home' }
  },
  {
    path: '/realtime',
    component: () => import('./Examples/RealTime/RealTime.vue'),
    meta: { title: 'Realtime' }
  },
  {
    path: '/icons',
    component: () => import('./Examples/Icons/IconView.vue'),
    meta: { title: 'Icons' }
  },
  {
    path: "/modals",
    component: () => import('./Examples/Modals/ModalsView.vue'),
    meta: { title: 'Modals' }
  },
  {
    path: "/errors",
    meta: { title: 'Errors' },
    children: [
      {
        path: 'serverError',
        component: () => import('./Examples/Errors/ServerError.vue'),
        meta: { title: 'Server exception' }
      },
      {
        path: 'clientError',
        component: () => import('./Examples/Errors/ClientError.vue'),
        meta: { title: 'Client error' }
      }
    ]
  },
]