import { defineConfig } from "vite";
import react from "@vitejs/plugin-react";

// Relative base so the built app works from any IIS site or virtual directory.
export default defineConfig({
  base: "./",
  plugins: [react()],
  server: { port: 5173 },
  preview: { port: 4173 },
});
