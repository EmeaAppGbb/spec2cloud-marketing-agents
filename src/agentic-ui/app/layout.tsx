import { CopilotKitProvider } from "./components/CopilotKitProvider";
import "@copilotkit/react-ui/styles.css";
import "./globals.css";

export default function RootLayout({ children }: {children: React.ReactNode}) {
  return (
    <html lang="en">
      <head>
        <meta charSet="utf-8" />
        <meta name="viewport" content="width=device-width, initial-scale=1" />
        <title>Marketing Campaign Studio</title>
      </head>
      <body>
        <CopilotKitProvider>
          {children}
        </CopilotKitProvider>
      </body>
    </html>
  );
}