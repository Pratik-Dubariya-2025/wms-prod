import { defineConfig } from 'vite';
import react from '@vitejs/plugin-react';
import tailwindcss from '@tailwindcss/vite';
import path from 'path';

// https://vite.dev/config/
export default defineConfig({
  plugins: [
    react(),
    tailwindcss(),
  ],
  resolve: {
    alias: {
      '@': path.resolve(__dirname, './src'),
    },
  },
  server: {
    port: 5173,
    proxy: {
      // Dev server proxies /api to the locally-run .NET API (http profile),
      // so the browser stays same-origin (no CORS, no self-signed cert).
      '/api': {
        target: 'http://localhost:5247',
        changeOrigin: true,
        secure: false,
      },
      // SignalR hub — WebSocket proxy for real-time communication
      '/hubs': {
        target: 'http://localhost:5247',
        changeOrigin: true,
        secure: false,
        ws: true,
      },
    },
  },
});
