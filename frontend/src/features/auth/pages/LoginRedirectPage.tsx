import { useEffect, useState } from 'react';
import { Navigate } from 'react-router-dom';
import { keycloak } from '@/shared/lib/keycloak';
import { useAuthStore } from '@/stores/authStore';
import { vendorPortalApi } from '@/features/vendors/api/vendorApi';

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
    return (
      <div className="flex flex-col items-center justify-center min-h-screen gap-4 text-center p-6">
        <p className="text-lg font-semibold text-slate-800">Vendor account not linked</p>
        <p className="text-sm text-slate-500 max-w-sm">
          Your account has not been linked to a vendor profile yet. Please contact your administrator.
        </p>
        <button
          className="px-4 py-2 text-sm bg-slate-800 text-white rounded-lg hover:bg-slate-700"
          onClick={() => keycloak.logout({ redirectUri: window.location.origin })}
        >
          Sign Out
        </button>
      </div>
    );
  }

  // Internal user
  return <Navigate to="/app/dashboard" replace />;
}
