import { Ref } from "vue"

export type TabData = { header: string, id?: string } & Record<string, any>
export type TabDataWithId = Required<TabData>

export type TabProvider = {
  tabs: Ref<TabDataWithId[]>
  active: Ref<TabDataWithId | undefined>,
  add: (tab: TabDataWithId) => void,
  remove: (id: string) => void,
  update: (id: string, props: Record<string, any>) => void,
  setActive: (id: string) => void
}