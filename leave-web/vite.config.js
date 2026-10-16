import { defineConfig } from 'vite';
import react from '@vitejs/plugin-react';

// Dev proxy: forwards /api to the .NET API.
// If you use `dotnet run` with the default HTTPS profile (https://localhost:7010),
// the dev cert must be trusted: `dotnet dev-certs https --trust` (Windows).
export default defineConfig({
  plugins: [react()],
  server: {
    port: 5173,
    proxy: {
      '/api': {
        target: 'https://localhost:7010',
        changeOrigin: true,
        secure: false,
      },
    },
  },
  preview: {
    port: 4173,
  },
});
