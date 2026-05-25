import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'

export default defineConfig({
  plugins: [react()],
  server: {
    host: '0.0.0.0',
    port: 5173,
    allowedHosts: [
      'soothing-kindness-production-f3dd.up.railway.app',
      'all'
    ]
  },
  preview: {
    host: '0.0.0.0',
    port: 5173,
    allowedHosts: [
      'soothing-kindness-production-f3dd.up.railway.app',
      'all'
    ]
  }
})