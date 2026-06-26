import { StrictMode } from 'react';
import { createRoot } from 'react-dom/client';
import './index.css';
import App from './App.tsx';
import { keycloak } from './shared/lib/keycloak';
import { apiClient } from './shared/lib/axios';
import { useAuthStore } from './stores/authStore';

async function bootstrap() {
  try {
    const authenticated = await keycloak.init({
      onLoad: 'check-sso',
      silentCheckSsoRedirectUri: `${window.location.origin}/silent-check-sso.html`,
      pkceMethod: 'S256',
    });

    if (authenticated) {
      try {
        const res = await apiClient.get('/auth/profile');
        useAuthStore.getState().setUser(res.data.data);
      } catch {
        useAuthStore.getState().clearUser();
      }
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
