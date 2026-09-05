import { defineConfig } from 'vite';

export default defineConfig({
  build: {
    lib: {
      entry: 'src/main.ts',
      formats: ['iife'],
      name: 'WebRTCBridgeModule',
      fileName: () => 'webrtc-web-client.js'
    },
    outDir: '../../Assets/WebGLTemplates/WebRTCTest/scripts',
    emptyOutDir: false,
    minify: true // Turn on/off minification - For testing only
  }
});
