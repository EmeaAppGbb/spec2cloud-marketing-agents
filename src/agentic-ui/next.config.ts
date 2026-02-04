import type { NextConfig } from "next";
import path from "path";

const nextConfig: NextConfig = {
  output: 'standalone',
  serverExternalPackages: ['pino', 'thread-stream', '@azure/monitor-opentelemetry', '@opentelemetry/api'],
  turbopack: {
    root: path.resolve(__dirname),
  },
};

export default nextConfig;
