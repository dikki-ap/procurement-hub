import { StrictMode } from 'react';
import { createRoot } from 'react-dom/client';
import './index.css';
import App from './App.tsx';
import { keycloak } from './shared/lib/keycloak';
import { apiClient } from './shared/lib/axios';
import { useAuthStore } from './stores/authStore';

const VENDOR_ROLES = ['vendor_admin', 'vendor_staff'];

async function bootstrap() {
  try {
    const authenticated = await keycloak.init({
      onLoad: 'check-sso',
      silentCheckSsoRedirectUri: `${window.location.origin}/silent-check-sso.html`,
      pkceMethod: 'S256',
    });

    if (authenticated && keycloak.tokenParsed) {
      const t = keycloak.tokenParsed as Record<string, unknown> & {
        sub?: string;
        email?: string;
        name?: string;
        preferred_username?: string;
        resource_access?: Record<string, { roles: string[] }>;
        company_id?: string;
        vendor_id?: string;
      };
      const clientId = import.meta.env.VITE_KEYCLOAK_CLIENT_ID ?? 'procurehub-web';
      const roles: string[] = t.resource_access?.[clientId]?.roles ?? [];
      const isVendor = roles.some(r => VENDOR_ROLES.includes(r));

      // Start with what the JWT provides (company_id may be absent)
      let companyId: string = t.company_id ?? '';

      // For internal users, resolve companyId from the backend if JWT doesn't carry it
      if (!isVendor && !companyId) {
        try {
          const res = await apiClient.get<{ data: { companyId: string } }>('/users/me');
          companyId = res.data.data.companyId;
        } catch {
          // non-fatal: master data will be empty but auth still works
        }
      }

      useAuthStore.getState().setUser({
        id:        t.sub             ?? '',
        email:     t.email           ?? '',
        fullName:  t.name            ?? t.preferred_username ?? '',
        role:      roles[0]          ?? '',
        companyId,
        vendorId:  t.vendor_id,
        roles,
      });
    } else {
      useAuthStore.getState().clearUser();
    }
  } catch {
    useAuthStore.getState().clearUser();
  } finally {
    useAuthStore.getState().setInitialized(true);
  }

  createRoot(document.getElementById('root')!).render(
    <StrictMode>
      <App />
    </StrictMode>,
  );
}

bootstrap();
