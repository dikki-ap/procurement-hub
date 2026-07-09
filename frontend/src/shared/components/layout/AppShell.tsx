import { useEffect } from 'react';
import { Outlet, useLocation } from 'react-router-dom';
import { Sidebar } from './Sidebar';
import { Topbar } from './Topbar';
import { useAuth } from '@/features/auth/hooks/useAuth';
import { useUIStore } from '@/stores/uiStore';

export const AppShell = () => {
  useAuth();
  const location = useLocation();
  const { sidebarOpen, setSidebarOpen } = useUIStore();

  // Close sidebar by default on mobile on first load
  useEffect(() => {
    if (window.innerWidth < 768) {
      setSidebarOpen(false);
    }
  }, [setSidebarOpen]);

  return (
    <div className="flex h-screen overflow-hidden bg-slate-50">
      {/* Mobile overlay backdrop */}
      {sidebarOpen && (
        <div
          className="fixed inset-0 z-40 bg-black/50 md:hidden"
          onClick={() => setSidebarOpen(false)}
        />
      )}

      <Sidebar />

      <div className="flex flex-col flex-1 overflow-hidden min-w-0">
        <Topbar />
        <main className="flex-1 overflow-y-auto bg-slate-50 p-4 sm:p-6">
          <div
            key={location.pathname}
            className="animate-[page-enter_0.2s_ease-out]"
          >
            <Outlet />
          </div>
        </main>
      </div>
    </div>
  );
};
