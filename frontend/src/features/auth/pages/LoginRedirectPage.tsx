import { useEffect } from 'react';
import { Navigate } from 'react-router-dom';
import { keycloak } from '@/shared/lib/keycloak';
import { useAuthStore } from '@/stores/authStore';

export default function LoginRedirectPage() {
  const { isInitialized, user } = useAuthStore();

  useEffect(() => {
    if (isInitialized && !keycloak.authenticated) {
      keycloak.login({ redirectUri: `${window.location.origin}/app/dashboard` });
    }
  }, [isInitialized]);

  if (keycloak.authenticated && user) {
    return <Navigate to="/app/dashboard" replace />;
  }

  return (
    <div className="flex items-center justify-center min-h-screen">
      <p className="text-muted-foreground">Redirecting to login...</p>
    </div>
  );
}
