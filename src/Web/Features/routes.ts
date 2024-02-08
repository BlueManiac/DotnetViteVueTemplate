import { RouteRecordRaw } from 'vue-router'
import { exampleRoutes } from './Examples/routes'

declare module 'vue-router' {
  interface RouteMeta {
    title?: string
    id?: string
    parentId?: string,
    fullPath?: string
  }
}

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