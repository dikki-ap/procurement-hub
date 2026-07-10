import { useEffect } from 'react';
import { Navigate } from 'react-router-dom';
import { keycloak } from '@/shared/lib/keycloak';
import { useAuthStore } from '@/stores/authStore';

function getHomeRoute(roles: string[], vendorId?: string): string {
  const isVendor = roles.some(r => ['vendor_admin', 'vendor_staff'].includes(r));
  if (isVendor && vendorId) {
    return `/app/vendor-portal/${vendorId}/profile`;
  }
  return '/app/dashboard';
}

export default function LoginRedirectPage() {
  const { isInitialized, user } = useAuthStore();

  useEffect(() => {
    if (isInitialized && !keycloak.authenticated) {
      keycloak.login({ redirectUri: window.location.origin });
    }
  }, [isInitialized]);

  if (keycloak.authenticated && user) {
    return <Navigate to={getHomeRoute(user.roles, user.vendorId)} replace />;
  }

  return (
    <div className="flex items-center justify-center min-h-screen">
      <p className="text-muted-foreground">Redirecting to login...</p>
    </div>
  );
}
