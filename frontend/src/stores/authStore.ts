import { create } from 'zustand';
import { persist } from 'zustand/middleware';

interface UserProfile {
  id: string;
  email: string;
  fullName: string;
  role: string;
  companyId: string;
  vendorId?: string;
  roles: string[];
}

interface AuthState {
  user: UserProfile | null;
  isInitialized: boolean;
  setUser: (user: UserProfile) => void;
  clearUser: () => void;
  setInitialized: (v: boolean) => void;
  hasAnyRole: (...roles: string[]) => boolean;
}

export const useAuthStore = create<AuthState>()(
  persist(
    (set, get) => ({
      user: null,
      isInitialized: false,
      setUser: (user) => set({ user }),
      clearUser: () => set({ user: null }),
      setInitialized: (v) => set({ isInitialized: v }),
      hasAnyRole: (...roles) => {
        const u = get().user;
        if (!u) return false;
        return roles.some((r) => u.roles.includes(r));
      },
    }),
    { name: 'auth-storage', partialize: (s) => ({ user: s.user }) }
  )
);
