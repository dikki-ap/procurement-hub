import { useEffect, useState } from 'react';
import { Navigate } from 'react-router-dom';
import { keycloak } from '@/shared/lib/keycloak';
import { useAuthStore } from '@/stores/authStore';
import { vendorPortalApi } from '@/features/vendors/api/vendorApi';
import VendorNotLinkedPage from './VendorNotLinkedPage';

const VENDOR_ROLES = ['vendor_admin', 'vendor_staff'];

export default function LoginRedirectPage() {
  const { isInitialized, user, setUser } = useAuthStore();
  const [resolvedVendorId, setResolvedVendorId] = useState<string | null>(null);
  const [resolveError, setResolveError]         = useState(false);

  const isVendor = user?.roles.some(r => VENDOR_ROLES.includes(r)) ?? false;

  useEffect(() => {
    if (isInitialized && !keycloak.authenticated) {
      keycloak.login({ redirectUri: window.location.origin });
      return;
    }

    // Vendor user with no vendorId in token — fetch it from the API
    if (keycloak.authenticated && user && isVendor && !user.vendorId) {
      vendorPortalApi.getMyVendorId()
        .then((id) => {
          setUser({ ...user, vendorId: id });
          setResolvedVendorId(id);
        })
        .catch(() => setResolveError(true));
    }
  }, [isInitialized, user, isVendor]);

  // Not yet initialized — show spinner
  if (!isInitialized) {
    return (
      <div className="flex items-center justify-center min-h-screen">
        <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-primary" />
      </div>
    );
  }

  // Waiting for Keycloak login redirect
  if (!keycloak.authenticated) {
    return (
      <div className="flex items-center justify-center min-h-screen">
        <p className="text-muted-foreground">Redirecting to login...</p>
      </div>
    );
  }

  if (!user) return null;

  // Vendor user — determine destination
  if (isVendor) {
    const vid = user.vendorId ?? resolvedVendorId;

    if (vid) {
      return <Navigate to={`/app/vendor-portal/${vid}/profile`} replace />;
    }

    // Still resolving vendorId from API
    if (!resolveError) {
      return (
        <div className="flex items-center justify-center min-h-screen">
          <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-primary" />
        </div>
      );
    }

    // API call failed — vendor account not linked yet
    return <VendorNotLinkedPage />;
  }

  // Internal user
  return <Navigate to="/app/dashboard" replace />;
}
