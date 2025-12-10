import { defineConfig } from 'vite'
import vue from '@vitejs/plugin-vue'

// https://vitejs.dev/config/
export default defineConfig({
    plugins: [vue()],
    server: {
        proxy: {
            // 捕获所有以 /api 开头的请求
            '/api': {
                target: 'http://localhost:5000', // 转发到你的 .NET 后端
                changeOrigin: true, // 允许跨域
                // 如果后端路径本身就包含 /api，则不需要 rewrite
                // 如果后端是 http://localhost:5000/Auth/login，则需要去掉 /api
                // 根据你提供的 URL，不需要 rewrite，直接转发即可
            }
        }
    }
})