import { PanelLeft, LogOut } from 'lucide-react';
import { Button } from '@/components/ui/button';
import { useAuthStore } from '@/stores/authStore';
import { useUIStore } from '@/stores/uiStore';
import { keycloak } from '@/shared/lib/keycloak';
import { NotificationPanel } from '@/features/notifications/components/NotificationPanel';

export const Topbar = () => {
  const { user } = useAuthStore();
  const { toggleSidebar, toggleSidebarCollapse } = useUIStore();

  const initials = user?.fullName
    ? user.fullName
        .split(' ')
        .slice(0, 2)
        .map((part) => part[0]?.toUpperCase() ?? '')
        .join('')
    : '?';

  const roleLabel = user?.role
    ? user.role.replace(/_/g, ' ').replace(/\b\w/g, (c) => c.toUpperCase())
    : '';

  const handleHamburger = () => {
    if (window.innerWidth < 768) {
      toggleSidebar();
    } else {
      toggleSidebarCollapse();
    }
  };

  return (
    <header className="h-14 flex-shrink-0 bg-white border-b border-slate-100 shadow-[0_1px_3px_rgba(0,0,0,0.04)] flex items-center justify-between px-4">
      {/* Left: sidebar toggle */}
      <button
        onClick={handleHamburger}
        className="flex h-8 w-8 items-center justify-center rounded-md text-slate-500 hover:bg-slate-100 hover:text-slate-700 transition-colors duration-150"
        aria-label="Toggle sidebar"
      >
        <PanelLeft className="h-4 w-4" />
      </button>

      {/* Right: notifications + user info + logout */}
      <div className="flex items-center gap-3">
        <NotificationPanel />
        <div className="bg-blue-500 text-white rounded-full w-8 h-8 flex items-center justify-center text-xs font-semibold flex-shrink-0">
          {initials}
        </div>
        {user?.fullName && (
          <span className="text-sm font-medium text-slate-700 hidden sm:block">
            {user.fullName}
          </span>
        )}
        {roleLabel && (
          <span className="bg-slate-100 text-slate-600 text-xs px-2 py-0.5 rounded-full hidden sm:block">
            {roleLabel}
          </span>
        )}
        <Button
          variant="ghost"
          size="sm"
          onClick={() => keycloak.logout()}
          className="text-slate-500 hover:text-slate-700 gap-1.5"
        >
          <LogOut className="h-4 w-4" />
          <span className="hidden sm:inline text-xs">Logout</span>
        </Button>
      </div>
    </header>
  );
};
