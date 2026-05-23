import { createBrowserRouter } from "react-router-dom";

import App from "../App";
import LandingPage from "../pages/LandingPage";
import LoginPage from "../pages/LoginPage";
import HomePage from "../pages/HomePage";
import RegisterPage from "../pages/RegisterPage";
import ProtectedRoute from "../components/guards/ProtectedRoute";
import AdminRoute from "../components/guards/AdminRoute";
import MenuManagementPage from "../pages/MenuManagementPage";
import MenuBrowsePage from "../pages/MenuBrowsePage";
import OrdersPage from "../pages/OrdersPage";

const router = createBrowserRouter([
  {
    path: "/",
    element: <App />,
    children: [
      {
        index: true,
        element: <LandingPage />,
      },
      {
        path: "login",
        element: <LoginPage />,
      },
      // --- authenticated routes ---
      {
        element: <ProtectedRoute />,
        children: [
          {
            path: "home",
            element: <HomePage />,
          },
          {
            path: "menu",
            element: <MenuManagementPage />,
          },
          {
            path: "menu/browse",
            element: <MenuBrowsePage />,
          },
          {
            path: "orders",
            element: <OrdersPage />,
          },
        ],
      },
      // --- admin-only routes ---
      {
        element: <AdminRoute />,
        children: [
          {
            path: "register",
            element: <RegisterPage />,
          },
        ],
      },
    ],
  },
]);

export default router;

