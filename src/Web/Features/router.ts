import { watch } from 'vue'
import { createRouter, createWebHistory, RouteLocationNormalizedLoaded, RouteRecordRaw } from 'vue-router'
import { Profile } from './Auth/Profile'
import { routes } from './routes'

const routeMap = new Map<string, RouteRecordRaw>()

export const Router = createRouter({
  scrollBehavior: () => ({ left: 0, top: 0 }),
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: addRouteMetadata(routes),
  linkActiveClass: 'active'
})

export function setupAuthGuard(profile: Profile) {
  Router.beforeEach((to, from, next) => {
    const requiresAuth = to.meta.auth !== false // default is true

    if (requiresAuth && !profile.isLoggedIn.value) {
      next({ path: '/auth/login', query: { redirect: to.fullPath } })
    } else {
      next()
    }
  })

  // Redirect to login if user is logged out (e.g., logout in another tab)
  watch(profile.isLoggedIn, (isLoggedIn) => {
    if (!isLoggedIn) {
      const currentRoute = Router.currentRoute.value
      const requiresAuth = currentRoute.meta.auth !== false
      if (requiresAuth) {
        Router.push({ path: '/auth/login', query: { redirect: currentRoute.fullPath } })
      }
    }
  })
}

export const navigationRoutes = computed(() => Array.from(createNavigationRoutes(Router.options.routes)))

export const title = computed(() => Router.currentRoute.value.meta?.title)

export const routePath = computed(() => {
  const currentRoute = Router.currentRoute.value

  if (!currentRoute.meta.id) {
    return []
  }

  return Array.from(create(currentRoute))

  function* create(route: RouteLocationNormalizedLoaded | RouteRecordRaw | undefined): Generator<RouteRecordRaw> {
    if (!route?.meta) {
      return
    }

    if (route.meta?.parentId) {
      const parentRoute = routeMap.get(route.meta.parentId)

      yield* create(parentRoute)
    }

    yield routeMap.get(route.meta!.id!)!
  }
})

function addRouteMetadata(routes: RouteRecordRaw[], parentRoute?: RouteRecordRaw) {
  for (const route of routes) {
    route.meta ??= {}

    const id = route.meta.id = createUniqueId()

    route.meta.parentId = parentRoute?.meta?.id
    route.meta.fullPath = createFullPath(route, parentRoute)

    route.name ??= route.meta.fullPath

    routeMap.set(id, route)

    if (route.children && route.children.length > 0) {
      addRouteMetadata(route.children, route)
    }
  }

  return routes

  function createUniqueId() {
    return Math.random().toString(36).substring(2, 9)
  }

  function createFullPath(route: RouteRecordRaw, parentRoute: RouteRecordRaw | undefined) {
    if (!route.component)
      return undefined

    let path = route.path

    while (parentRoute) {
      path = parentRoute.path + '/' + path
      parentRoute = routeMap.get(parentRoute.meta?.parentId!)
    }

    return path
  }
}

function* createNavigationRoutes(routes: readonly RouteRecordRaw[]) {
  for (let route of routes) {
    if (route.meta?.title)
      yield route

    if (!route.meta?.fullPath && route.children) {
      for (let childRoute of route.children) {
        // Include root index pages
        if (childRoute.meta?.title && !childRoute.path)
          yield childRoute
      }
    }
  }
}