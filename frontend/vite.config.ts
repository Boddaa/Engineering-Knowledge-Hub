import { defineConfig, loadEnv } from 'vite';
import react from '@vitejs/plugin-react';
import path from 'path';

// https://vite.dev/config/
export default defineConfig(({ mode }) => {
  const env = loadEnv(mode, process.cwd(), '');
  const rawTarget = env.VITE_BACKEND_TARGET || env.VITE_API_BASE_URL || 'http://localhost:5245';
  // Strip trailing /api or slash to obtain the backend server host for the reverse proxy
  const backendTarget = rawTarget.replace(/\/api\/?$/, '').replace(/\/+$/, '') || 'http://localhost:5245';

  return {
    plugins: [react()],
    resolve: {
      alias: {
        '@': path.resolve(__dirname, './src'),
      },
    },
    server: {
      port: 3000,
      proxy: {
        '/api': {
          target: backendTarget,
          changeOrigin: true,
          secure: false,
        },
      },
    },
  };
});
