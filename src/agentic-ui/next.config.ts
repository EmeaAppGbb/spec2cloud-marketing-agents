import type { NextConfig } from "next";
import path from "path";

const nextConfig: NextConfig = {
  output: 'standalone',
  serverExternalPackages: ['pino', 'thread-stream'],
  turbopack: {
    root: path.resolve(__dirname),
  },
};

export default nextConfig;
