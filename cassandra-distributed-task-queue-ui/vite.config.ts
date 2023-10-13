import react from "@vitejs/plugin-react";
import { defineConfig } from "vite";

// https://vitejs.dev/config/
export default defineConfig(({ mode }) => {
    const isDevMode = mode === "dev";
    return {
        plugins: [react()],
        define: process.env.VITEST
            ? {}
            : {
                  "process.env.API": isDevMode ? "'fake'" : "'real'",
                  "process.env.enableReactTesting": "true",
                  global: "window",
              },
        server: {
            proxy: {
                "/remote-task-queue": "http://localhost:4413/",
            },
        },
        test: {
            include: ["./tests/**/*.tsx"],
            globals: false,
            environment: "jsdom",
        },
    };
});
