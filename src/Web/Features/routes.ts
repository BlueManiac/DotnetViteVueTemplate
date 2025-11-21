import { RouteRecordRaw } from 'vue-router'
import { routes as autoRoutes } from 'vue-router/auto-routes'
import { exampleRoutes } from './Examples/routes'
import HomeView from './Home/HomeView.vue'
import NotFound from '/Components/Views/NotFound.vue'

declare module 'vue-router' {
  interface RouteMeta {
    title?: string
    id?: string
    parentId?: string
    fullPath?: string
    centered?: boolean
    auth?: boolean
  }
}

export const routes: RouteRecordRaw[] = [
  {
    path: '/',
    component: HomeView,
    meta: { title: 'Home' }
  },
  ...exampleRoutes,
  ...autoRoutes,
  {
    path: '/:pathMatch(.*)*',
    component: NotFound,
    meta: { auth: false }
  }
]