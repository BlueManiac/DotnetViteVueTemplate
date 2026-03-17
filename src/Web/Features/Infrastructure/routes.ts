import { RouteRecordRaw } from 'vue-router'
import SitemapView from './Sitemap/Pages/index.vue'

export const infrastructureRoutes: RouteRecordRaw[] = [
  {
    path: '/infrastructure',
    meta: { title: 'Infrastructure' },
    children: [
      {
        path: 'sitemap',
        component: SitemapView,
        meta: { title: 'Sitemap', auth: false }
      }
    ]
  }
]
