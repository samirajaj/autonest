import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { BrowserRouter, Navigate, Route, Routes } from "react-router-dom";
import { AuthProvider, useAuth } from "./AuthContext";
import { Shell } from "../shared/components/Shell";
import type { Role } from "../shared/types";
import { WelcomePage } from "../features/home/WelcomePage";
import { PrivacyPage, NotFoundPage } from "../features/home/StaticPages";
import { AuthPage, ConfirmEmailPage } from "../features/auth/AuthPage";
import { CarsPage } from "../features/cars/CarsPage";
import { CarDetailsPage } from "../features/cars/CarDetailsPage";
import { FavoritesPage } from "../features/favorites/FavoritesPage";
import { RequestsPage } from "../features/requests/RequestsPage";
import { ProfilePage } from "../features/profile/ProfilePage";
import { DashboardPage } from "../features/dashboard/DashboardPage";
import { CompanyVehiclesPage } from "../features/company/CompanyVehiclesPage";
import { CompanyRequestsPage } from "../features/company/CompanyRequestsPage";
import { AdminUsersPage } from "../features/admin/AdminUsersPage";
import { AdminPlansPage } from "../features/admin/AdminPlansPage";
import { AdminPointsPage } from "../features/admin/AdminPointsPage";

const query = new QueryClient({
  defaultOptions: { queries: { staleTime: 30_000, retry: 1 } },
});

export function Guard({
  roles,
  children,
}: {
  roles?: Role[];
  children: React.ReactNode;
}) {
  const { session } = useAuth();
  if (!session) return <Navigate to="/auth" replace />;
  if (roles && !roles.includes(session.role))
    return (
      <Navigate
        to={session.role === "Customer" ? "/cars" : "/dashboard"}
        replace
      />
    );
  return children;
}

function Router() {
  return (
    <Routes>
      <Route element={<Shell />}>
        <Route index element={<WelcomePage />} />
        <Route path="cars" element={<CarsPage />} />
        <Route path="cars/:id" element={<CarDetailsPage />} />
        <Route path="auth" element={<AuthPage />} />
        <Route path="auth/confirm-email" element={<ConfirmEmailPage />} />
        <Route path="privacy" element={<PrivacyPage />} />
        <Route
          path="favorites"
          element={
            <Guard roles={["Customer"]}>
              <FavoritesPage />
            </Guard>
          }
        />
        <Route
          path="requests"
          element={
            <Guard roles={["Customer"]}>
              <RequestsPage />
            </Guard>
          }
        />
        <Route
          path="profile"
          element={
            <Guard roles={["Customer"]}>
              <ProfilePage />
            </Guard>
          }
        />
        <Route
          path="dashboard"
          element={
            <Guard roles={["Company", "Admin"]}>
              <DashboardPage />
            </Guard>
          }
        />
        <Route
          path="company/cars"
          element={
            <Guard roles={["Company"]}>
              <CompanyVehiclesPage />
            </Guard>
          }
        />
        <Route
          path="company/requests"
          element={
            <Guard roles={["Company"]}>
              <CompanyRequestsPage />
            </Guard>
          }
        />
        <Route
          path="admin/users"
          element={
            <Guard roles={["Admin"]}>
              <AdminUsersPage />
            </Guard>
          }
        />
        <Route
          path="admin/plans"
          element={
            <Guard roles={["Admin"]}>
              <AdminPlansPage />
            </Guard>
          }
        />
        <Route
          path="admin/points"
          element={
            <Guard roles={["Admin"]}>
              <AdminPointsPage />
            </Guard>
          }
        />
        <Route path="*" element={<NotFoundPage />} />
      </Route>
    </Routes>
  );
}

export function App() {
  return (
    <QueryClientProvider client={query}>
      <AuthProvider>
        <BrowserRouter>
          <Router />
        </BrowserRouter>
      </AuthProvider>
    </QueryClientProvider>
  );
}
