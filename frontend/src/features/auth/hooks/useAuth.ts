import { keycloak } from '@/shared/lib/keycloak';
import { useAuthStore } from '@/stores/authStore';

export const useAuth = () => {
  const { user, isInitialized } = useAuthStore();
  return {
    user,
    isInitialized,
    isAuthenticated: !!keycloak.authenticated,
    logout: () => keycloak.logout({ redirectUri: window.location.origin }),
  };
};
