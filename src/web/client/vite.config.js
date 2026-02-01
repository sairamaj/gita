import path from "path";
import { defineConfig } from "vite";
import react from "@vitejs/plugin-react";

export default defineConfig({
  plugins: [react()],
  server: {
    port: 5173,
    fs: {
      allow: [path.resolve(__dirname, "..", "..", "..")],
    },
  },
  build: {
    // Increase chunk size warning limit for large audio files
    chunkSizeWarningLimit: 20000,
    rollupOptions: {
      output: {
        // Don't include large assets in the bundle
        manualChunks: undefined,
      },
    },
  },
  // Ensure public directory assets are copied correctly
  publicDir: 'public',
});
