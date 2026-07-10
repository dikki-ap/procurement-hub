import { Navigate, Outlet } from 'react-router-dom';
import { keycloak } from '@/shared/lib/keycloak';
import { useAuthStore } from '@/stores/authStore';

interface Props {
  requiredRoles?: string[];
}

export const ProtectedRoute = ({ requiredRoles }: Props) => {
  const { isInitialized, user } = useAuthStore();

  if (!isInitialized) {
    return (
      <div className="flex items-center justify-center min-h-screen">
        <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-primary" />
      </div>
    );
  }

  if (!keycloak.authenticated) {
    keycloak.login({ redirectUri: window.location.origin });
    return null;
  }

  if (!user) {
    return (
      <div className="flex items-center justify-center min-h-screen">
        <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-primary" />
      </div>
    );
  }

  if (requiredRoles?.length && !requiredRoles.some((r) => user.roles.includes(r))) {
    return <Navigate to="/unauthorized" replace />;
  }

  return <Outlet />;
};
