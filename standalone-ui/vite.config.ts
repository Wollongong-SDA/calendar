import { defineConfig } from 'vite';
import plugin from '@vitejs/plugin-vue';

// https://vitejs.dev/config/
export default defineConfig({
    plugins: [plugin()],
    build: {
        rollupOptions: {
            output: {
                manualChunks: {
                    deviceDetector: ['device-detector-js'],
                    vue: ['vue'],
                    ui: ['primevue', '@iconify/vue']
                }
            }
        }
    }
})
