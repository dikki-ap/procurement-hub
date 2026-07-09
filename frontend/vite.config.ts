import { defineConfig } from 'vite';
import react from '@vitejs/plugin-react';
import tailwindcss from '@tailwindcss/vite';
import path from 'path';

export default defineConfig({
  plugins: [tailwindcss(), react()],
  resolve: {
    alias: { '@': path.resolve(__dirname, './src') },
  },
  server: {
    host: '127.0.0.1',
    port: 5173,
    proxy: {
      '/api': { target: 'http://localhost:8080', changeOrigin: true, secure: false },
      '/hubs': { target: 'http://localhost:8080', changeOrigin: true, ws: true },
    },
  },
  build: {
    outDir: 'dist',
    sourcemap: false,
    rollupOptions: {
      output: {
        manualChunks: (id) => {
          if (['react', 'react-dom', 'react-router-dom'].some((p) => id.includes(`/${p}/`))) return 'vendor';
          if (id.includes('@radix-ui')) return 'ui';
          if (id.includes('recharts')) return 'charts';
          if (id.includes('@tanstack/react-query')) return 'query';
          return undefined;
        },
      },
    },
  },
});
