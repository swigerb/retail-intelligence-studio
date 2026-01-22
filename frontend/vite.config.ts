import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'

// Get the server URL from Aspire service discovery or fall back to localhost:5347
// Aspire provides the URL in format: services__server__http__0 or services__server__https__0
const getServerUrl = (): string => {
  // Try Aspire service discovery first (format: http://localhost:PORT)
  const aspireHttpUrl = process.env['services__server__http__0'];
  const aspireHttpsUrl = process.env['services__server__https__0'];
  
  if (aspireHttpUrl) {
    console.log(`Using Aspire service discovery: ${aspireHttpUrl}`);
    return aspireHttpUrl;
  }
  if (aspireHttpsUrl) {
    console.log(`Using Aspire service discovery (HTTPS): ${aspireHttpsUrl}`);
    return aspireHttpsUrl;
  }
  
  // Fallback to default local development URL
  const fallbackUrl = 'http://localhost:5347';
  console.log(`Using fallback URL: ${fallbackUrl}`);
  return fallbackUrl;
}

// https://vite.dev/config/
export default defineConfig({
  plugins: [react()],
  server: {
    port: parseInt(process.env.PORT || '5173'),
    host: true,
    proxy: {
      // Proxy API calls to the .NET backend
      '/api': {
        target: getServerUrl(),
        changeOrigin: true,
        secure: false
      },
      // Proxy health endpoints to the .NET backend
      '/health': {
        target: getServerUrl(),
        changeOrigin: true,
        secure: false
      },
      '/alive': {
        target: getServerUrl(),
        changeOrigin: true,
        secure: false
      }
    }
  },
  build: {
    outDir: 'dist',
    emptyOutDir: true
  },
  test: {
    globals: true,
    environment: 'jsdom',
    setupFiles: './src/test/setup.ts',
    css: true,
  }
})
