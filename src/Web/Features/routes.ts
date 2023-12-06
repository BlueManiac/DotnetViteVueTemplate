import { RouteRecordRaw } from 'vue-router'
import { exampleRoutes } from './Examples/routes'

export const baseRoutes: RouteRecordRaw[] = [
  {
    path: '/',
    component: () => import('./Home/HomeView.vue'),
    meta: { title: 'Home' }
  },
  ...exampleRoutes,
  {
    path: '/login',
    component: () => import('./Auth/LoginView.vue'),
  },
  {
    path: '/:pathMatch(.*)*',
    component: () => import('/Components/Views/NotFound.vue'),
  }
]