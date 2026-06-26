import { Outlet, useLocation } from 'react-router-dom';
import { Sidebar } from './Sidebar';
import { Topbar } from './Topbar';
import { useAuth } from '@/features/auth/hooks/useAuth';

export const AppShell = () => {
  useAuth();
  const location = useLocation();

  return (
    <div className="flex h-screen overflow-hidden bg-slate-50">
      <Sidebar />
      <div className="flex flex-col flex-1 overflow-hidden min-w-0">
        <Topbar />
        <main className="flex-1 overflow-y-auto bg-slate-50 p-6">
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
