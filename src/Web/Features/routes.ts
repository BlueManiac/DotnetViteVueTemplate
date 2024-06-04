import { RouteRecordRaw } from 'vue-router'
import { exampleRoutes } from './Examples/routes'
import { routes as autoRoutes } from 'vue-router/auto-routes'

declare module 'vue-router' {
  interface RouteMeta {
    title?: string
    id?: string
    parentId?: string
    fullPath?: string
    centered?: boolean
  }
}

export const routes: RouteRecordRaw[] = [
  {
    path: '/',
    component: () => import('./Home/HomeView.vue'),
    meta: { title: 'Home' }
  },
  ...exampleRoutes,
  ...autoRoutes,
  {
    path: '/:pathMatch(.*)*',
    component: () => import('/Components/Views/NotFound.vue'),
  }
]