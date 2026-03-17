import { RouteRecordRaw } from 'vue-router'
import { routes as autoRoutes } from 'vue-router/auto-routes'
import { exampleRoutes } from './Features/Examples/routes'
import HomeView from './Features/Home/HomeView.vue'
import { infrastructureRoutes } from './Features/Infrastructure/routes'
import NotFound from '/Components/Views/NotFound.vue'

declare module 'vue-router' {
  interface RouteMeta {
    title?: string
    id?: string
    parentId?: string
    fullPath?: string
    display?: 'centered' | 'full'
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
  ...infrastructureRoutes,
  ...autoRoutes,
  {
    path: '/:pathMatch(.*)*',
    component: NotFound,
    meta: { auth: false }
  }
]