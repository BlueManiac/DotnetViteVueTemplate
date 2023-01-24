import { ref } from 'vue'
import { createRouter, createWebHistory, RouteRecordRaw } from 'vue-router'
import { applicationName } from './info'
import { baseRoutes } from './routes'

export const Router = createRouter({
  scrollBehavior: () => ({ left: 0, top: 0 }),
  history: createWebHistory(),
  routes: baseRoutes,
  linkActiveClass: 'active'
})

export const routes = ref(createRouteRecords(baseRoutes))

Router.beforeEach(to => {
  document.title = to.meta?.title
    ? to.meta.title + " - " + applicationName
    : applicationName
})

function createRouteRecords(routes?: RouteRecordRaw[], parentRoutePath?: string) {
  if (!routes?.length)
    return null

  const result = []

  for (const route of routes) {
    if (!route.meta?.title)
      continue

    result.push({
      path: route.component
        ? parentRoutePath
          ? parentRoutePath + '/' + route.path
          : route
        : null,
      children: createRouteRecords(route.children, route.path),
      meta: route.meta,
      id: createUniqueId()
    })
  }

  return result

  function createUniqueId() {
    return Math.random().toString(36).substring(2, 9)
  }
}