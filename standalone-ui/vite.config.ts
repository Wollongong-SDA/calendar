import { defineConfig } from 'vite';
import plugin from '@vitejs/plugin-vue';

// https://vitejs.dev/config/
export default defineConfig({
    plugins: [plugin()],
    define: {
        'APP_VERSION': JSON.stringify(process.env.npm_package_version)
    }
})
