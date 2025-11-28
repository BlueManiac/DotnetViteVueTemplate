import Vue from '@vitejs/plugin-vue'
import path from "node:path"
import AutoImport from 'unplugin-auto-import/vite'
import IconsResolver from 'unplugin-icons/resolver'
import Icons from 'unplugin-icons/vite'
import ViteComponents from 'unplugin-vue-components/vite'
import VueRouter from 'unplugin-vue-router/vite'
import { UserConfig } from 'vite'
import MkCert from 'vite-plugin-mkcert'
import Inspector from 'vite-plugin-vue-inspector'
import { viteRuntimeErrorOverlayPlugin } from './Util/Plugins/vite-runtime-error-plugin'

export default ({ mode }: { mode: string }): UserConfig => {
  const iconsResolver = IconsResolver({
    prefix: false,
    enabledCollections: ['carbon', 'mdi', 'svg-spinners'],
  })

  return {
    resolve: {
      alias: {
        'vue-original': 'vue',
        'vue': path.resolve(__dirname, 'Util/Client/di.ts'),
      }
    },
    plugins: [
      VueRouter({
        dts: 'typed-router.d.ts',
        routesFolder: [{
          src: 'Pages'
        }, {
          // Match Features/Feature1/Pages/test.vue to feature1/test
          src: 'Features',
          filePatterns: '**/Pages/*',
          path(file: string) {
            const prefix = 'Features'
            const basePath = file.slice(file.lastIndexOf(prefix))
            const filePath = basePath
              .slice(prefix.length + 1)
              .replace('Pages/', '')
              .replace('/index.vue', '')
              .toLocaleLowerCase()

            return filePath
          },
        }],
        extendRoute(route) {
          route.addToMeta({
            filePath: route.component
          })
        },
        logs: false
      }),
      Vue(),
      ViteComponents({
        globs: ['Components/**/*.vue', 'Features/**/*.vue', '!Features/**/Pages/**'],
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
        imports: [
          {
            'vue': ['ref', 'computed']
          },
        ],
        resolvers: [
          iconsResolver
        ],
      }),
      MkCert(),
      Inspector({
        disableInspectorOnEditorOpen: true
      }),
      viteRuntimeErrorOverlayPlugin({
        includeServerStack: false
      })
    ],
    server: {
      proxy: {
        ...(mode === 'development' && {
          '/api': {
            target: 'https://localhost:7126/api',
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