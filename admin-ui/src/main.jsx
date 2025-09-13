import React from "react";
import ReactDOM from "react-dom/client";
import "./index.css";
import AppRoutes from "./routes";
import { ToastProvider } from "./ui-toast";

ReactDOM.createRoot(document.getElementById("root")).render(
  <React.StrictMode>
    <ToastProvider>
      <AppRoutes />
    </ToastProvider>
  </React.StrictMode>
);
