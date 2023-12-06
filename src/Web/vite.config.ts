import vue from '@vitejs/plugin-vue'
import AutoImport from 'unplugin-auto-import/vite'
import IconsResolver from 'unplugin-icons/resolver'
import Icons from 'unplugin-icons/vite'
import ViteComponents from 'unplugin-vue-components/vite'
import { UserConfig } from 'vite'
import mkcert from 'vite-plugin-mkcert'
import Inspector from 'vite-plugin-vue-inspector'

export default ({ mode }): UserConfig => {
  const iconsResolver = IconsResolver({
    prefix: false,
    enabledCollections: ['carbon', 'mdi', 'svg-spinners'],
  })

  return {
    plugins: [
      vue({
        script: {
          defineModel: true,
          propsDestructure: true
        }
      }),
      ViteComponents({
        dirs: ['Components', 'Features'],
        resolvers: [
          iconsResolver
        ],
        dts: true
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
      mkcert(),
      Inspector({
        disableInspectorOnEditorOpen: true
      })
    ],
    server: {
      proxy: {
        ...(mode === 'development' && {
          '/api': {
            target: 'https://localhost:7126',
            changeOrigin: true,
            secure: false
          }
        })
      }
    },
    css: {
      transformer: 'lightningcss'
    },
    build: {
      cssMinify: 'lightningcss',
      target: 'esnext',
    }
  }
}