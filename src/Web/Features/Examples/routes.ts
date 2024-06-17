import { RouteRecordRaw } from 'vue-router'
import ClientError from './Errors/ClientError.vue'
import ServerError from './Errors/ServerError.vue'
import IconView from './Icons/IconView.vue'
import ModalsView from './Modals/ModalsView.vue'
import RealTime from './RealTime/RealTime.vue'
import TableView from './Tables/TableView.vue'

export const exampleRoutes: RouteRecordRaw[] = [
  {
    path: '/realtime',
    component: RealTime,
    meta: { title: 'Realtime' }
  },
  {
    path: '/icons',
    component: IconView,
    meta: { title: 'Icons' }
  },
  {
    path: '/modals',
    component: ModalsView,
    meta: { title: 'Modals' }
  },
  {
    path: '/tables',
    component: TableView,
    meta: { title: 'Tables' }
  },
  {
    path: '/errors',
    meta: { title: 'Errors' },
    children: [
      {
        path: 'serverError',
        component: ServerError,
        meta: { title: 'Server exception' }
      },
      {
        path: 'clientError',
        component: ClientError,
        meta: { title: 'Client error' }
      }
    ]
  }
]