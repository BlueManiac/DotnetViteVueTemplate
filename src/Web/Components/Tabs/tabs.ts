import { Ref } from "vue"

export type TabData = { header: string, id?: string } & Record<string, any>
export type TabDataWithId = Required<TabData>

export type TabProvider = {
  tabs: Ref<TabData[]>
  active: Ref<TabData>,
  add: (tab: TabData) => void,
  remove: (id: string) => void,
  update: (id: string, props: Record<string, any>) => void,
  setActive: (id: string) => void
}