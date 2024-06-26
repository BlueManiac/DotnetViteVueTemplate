import { createRouter, createWebHistory, RouteLocationNormalizedLoaded, RouteRecordRaw } from 'vue-router/auto'
import { routes } from './routes'

const routeMap = new Map<string, RouteRecordRaw>()

export const Router = createRouter({
  scrollBehavior: () => ({ left: 0, top: 0 }),
  history: createWebHistory(),
  routes: addRouteMetadata(routes),
  linkActiveClass: 'active'
})

export const navigationRoutes = computed(() => Array.from(createNavigationRoutes(Router.options.routes)))

export const title = computed(() => Router.currentRoute.value.meta?.title)

export const routePath = computed(() => {
  const currentRoute = Router.currentRoute.value

  if (!currentRoute.meta.id) {
    return []
  }

  return Array.from(create(currentRoute))

  function* create(route: RouteLocationNormalizedLoaded): Generator<RouteRecordRaw> {
    if (route.meta.parentId) {
      const parentRoute = routeMap[route.meta.parentId]

      yield* create(parentRoute)
    }

    yield routeMap[route.meta.id]
  }
})

function addRouteMetadata(routes: RouteRecordRaw[], parentRoute?: RouteRecordRaw) {
  for (const route of routes) {
    route.meta ??= {}

    const id = route.meta.id = createUniqueId()

    route.meta.parentId = parentRoute?.meta.id
    route.meta.fullPath = createFullPath(route, parentRoute)

    route.name ??= route.meta.fullPath

    routeMap[id] = route

    if (route.children && route.children.length > 0) {
      addRouteMetadata(route.children, route)
    }
  }

  return routes

  function createUniqueId() {
    return Math.random().toString(36).substring(2, 9)
  }

  function createFullPath(route: RouteRecordRaw, parentRoute: RouteRecordRaw) {
    if (!route.component)
      return null

    let path = route.path

    while (parentRoute) {
      path = parentRoute.path + '/' + path
      parentRoute = routeMap[parentRoute.meta.parentId]
    }

    return path
  }
}

function* createNavigationRoutes(routes: readonly RouteRecordRaw[]) {
  for (let route of routes) {
    if (route.meta?.title)
      yield route

    if (!route.meta.fullPath && route.children) {
      for (let childRoute of route.children) {
        // Include root index pages
        if (childRoute.meta?.title && !childRoute.path)
          yield childRoute
      }
    }
  }
}