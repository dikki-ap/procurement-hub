import { StrictMode } from 'react';
import { createRoot } from 'react-dom/client';
import './index.css';
import App from './App.tsx';
import { keycloak } from './shared/lib/keycloak';
import { useAuthStore } from './stores/authStore';

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
      useAuthStore.getState().setUser({
        id:        t.sub             ?? '',
        email:     t.email           ?? '',
        fullName:  t.name            ?? t.preferred_username ?? '',
        role:      roles[0]          ?? '',
        companyId: t.company_id      ?? '',
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
