import { ref } from 'vue'
import { createRouter, createWebHistory, RouteRecordRaw } from 'vue-router'
import { baseRoutes } from './routes'

const routeMap = new Map<string, RouteRecordRaw>()

addRouteMetadata(baseRoutes)

export const Router = createRouter({
  scrollBehavior: () => ({ left: 0, top: 0 }),
  history: createWebHistory(),
  routes: baseRoutes,
  linkActiveClass: 'active'
})

export const routes = ref(baseRoutes.filter(x => x.meta?.title))

export const title = computed(() => Router.currentRoute.value.meta?.title)

export const routePath = computed(() => {
  const currentRoute = Router.currentRoute.value

  if (!currentRoute.meta.id) {
    return []
  }

  return Array.from(create(currentRoute))

  function* create(route) {
    if (route.meta.parentId) {
      const parentRoute = routeMap[route.meta.parentId]

      yield* create(parentRoute)
    }

    yield routeMap[route.meta.id]
  }
})

function addRouteMetadata(routes: RouteRecordRaw[], parentRoute?: RouteRecordRaw) {
  for (const route of routes) {
    if (!route.meta)
      continue

    const id = route.meta.id = createUniqueId()

    route.meta.parentId = parentRoute?.meta.id
    route.meta.fullPath = route.component
      ? parentRoute?.path
        ? parentRoute.path + '/' + route.path
        : route.path
      : null

    routeMap[id] = route

    if (route.children && route.children.length > 0) {
      addRouteMetadata(route.children, route)
    }
  }

  function createUniqueId() {
    return Math.random().toString(36).substring(2, 9)
  }
}