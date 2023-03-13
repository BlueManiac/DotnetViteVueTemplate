import vue from '@vitejs/plugin-vue'
import ViteComponents from 'unplugin-vue-components/vite'
import Icons from 'unplugin-icons/vite'
import IconsResolver from 'unplugin-icons/resolver'
import AutoImport from 'unplugin-auto-import/vite'
import process from 'process'
import { ViteEjsPlugin } from "vite-plugin-ejs"
import { loadEnv } from 'vite'

export default ({ mode }) => {
  const env = { ...process.env, ...loadEnv(mode, process.cwd()) }

  const iconsResolver = IconsResolver({
    componentPrefix: "icon",
    enabledCollections: ["carbon", "mdi"],
  })

  return {
    plugins: [
      vue(),
      ViteComponents({
        dirs: ['Components', 'Features'],
        resolvers: [
          iconsResolver
        ]
      }),
      Icons({
        scale: 1,
        defaultClass: 'iconify'
      }),
      AutoImport({
        dts: 'auto-imports.d.ts',
        imports: {
          'vue': ['ref', 'computed']
        },
        resolvers: [
          iconsResolver
        ],
      }),
      ViteEjsPlugin({
        apiUrl: env.VITE_API_URL,
        applicationName: env.VITE_APPLICATION_NAME
      })
    ]
  }
}