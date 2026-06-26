import { NavLink } from 'react-router-dom';
import {
  Package,
  DollarSign,
  MapPin,
  CreditCard,
  Ruler,
  FolderOpen,
  LayoutDashboard,
  ChevronLeft,
  ChevronRight,
  Users,
} from 'lucide-react';
import { useUIStore } from '@/stores/uiStore';
import { useAuthStore } from '@/stores/authStore';

const masterDataLinks = [
  { to: '/app/master-data/materials', icon: Package, label: 'Materials' },
  { to: '/app/master-data/currencies', icon: DollarSign, label: 'Currencies' },
  { to: '/app/master-data/locations', icon: MapPin, label: 'Locations' },
  { to: '/app/master-data/payment-terms', icon: CreditCard, label: 'Payment Terms' },
  { to: '/app/master-data/uoms', icon: Ruler, label: 'Units of Measure' },
  { to: '/app/master-data/material-categories', icon: FolderOpen, label: 'Material Categories' },
];

export const Sidebar = () => {
  const { sidebarCollapsed, toggleSidebarCollapse } = useUIStore();
  const { user } = useAuthStore();
  const isSuperAdmin = user?.role === 'super_admin';

  return (
    <aside
      className="flex flex-col flex-shrink-0 bg-[#0f1729] text-slate-200 transition-[width] duration-300 ease-in-out overflow-hidden"
      style={{ width: sidebarCollapsed ? '64px' : '240px' }}
    >
      {/* Logo area */}
      <div className="flex items-center gap-3 h-14 px-3 flex-shrink-0 border-b border-white/8">
        <div className="flex-shrink-0 w-8 h-8 rounded-lg bg-blue-500 flex items-center justify-center text-white text-xs font-bold tracking-wide">
          PH
        </div>
        <span
          className="font-semibold text-sm text-white whitespace-nowrap transition-[opacity,width] duration-300 overflow-hidden"
          style={{ opacity: sidebarCollapsed ? 0 : 1, width: sidebarCollapsed ? 0 : 'auto' }}
        >
          Procure Hub
        </span>
      </div>

      {/* Navigation */}
      <nav className="flex-1 overflow-y-auto overflow-x-hidden py-3 space-y-0.5">
        <NavLink
          to="/app/dashboard"
          title="Dashboard"
          className={({ isActive }) =>
            `flex items-center gap-3 py-2.5 rounded-lg mx-2 text-sm transition-all duration-150 ${
              sidebarCollapsed ? 'px-[18px] justify-center' : 'px-3'
            } ${
              isActive
                ? 'border-l-2 border-blue-400 bg-blue-500/10 text-white pl-[10px]'
                : 'border-l-2 border-transparent text-slate-400 hover:bg-white/8 hover:text-white'
            }`
          }
        >
          <LayoutDashboard className="h-4 w-4 flex-shrink-0" />
          <span
            className="whitespace-nowrap overflow-hidden transition-[opacity,max-width] duration-300"
            style={{ opacity: sidebarCollapsed ? 0 : 1, maxWidth: sidebarCollapsed ? 0 : '200px' }}
          >
            Dashboard
          </span>
        </NavLink>

        {/* Vendors — visible to all internal users */}
        <div className="pt-3">
          <NavLink
            to="/app/vendors"
            title="Vendors"
            className={({ isActive }) =>
              `flex items-center gap-3 py-2.5 rounded-lg mx-2 text-sm transition-all duration-150 ${
                sidebarCollapsed ? 'px-[18px] justify-center' : 'px-3'
              } ${
                isActive
                  ? 'border-l-2 border-blue-400 bg-blue-500/10 text-white pl-[10px]'
                  : 'border-l-2 border-transparent text-slate-400 hover:bg-white/8 hover:text-white'
              }`
            }
          >
            <Users className="h-4 w-4 flex-shrink-0" />
            <span
              className="whitespace-nowrap overflow-hidden transition-[opacity,max-width] duration-300"
              style={{ opacity: sidebarCollapsed ? 0 : 1, maxWidth: sidebarCollapsed ? 0 : '200px' }}
            >
              Vendors
            </span>
          </NavLink>
        </div>

        {isSuperAdmin && (
          <div className="pt-3">
            {/* Group label */}
            <div
              className="overflow-hidden transition-[max-height,opacity] duration-300"
              style={{ maxHeight: sidebarCollapsed ? 0 : '40px', opacity: sidebarCollapsed ? 0 : 1 }}
            >
              <p className="px-5 pb-1 text-[10px] font-semibold text-slate-500 uppercase tracking-widest">
                Master Data
              </p>
            </div>

            {masterDataLinks.map(({ to, icon: Icon, label }) => (
              <NavLink
                key={to}
                to={to}
                title={label}
                className={({ isActive }) =>
                  `flex items-center gap-3 py-2.5 rounded-lg mx-2 text-sm transition-all duration-150 ${
                    sidebarCollapsed ? 'px-[18px] justify-center' : 'px-3'
                  } ${
                    isActive
                      ? 'border-l-2 border-blue-400 bg-blue-500/10 text-white pl-[10px]'
                      : 'border-l-2 border-transparent text-slate-400 hover:bg-white/8 hover:text-white'
                  }`
                }
              >
                <Icon className="h-4 w-4 flex-shrink-0" />
                <span
                  className="whitespace-nowrap overflow-hidden transition-[opacity,max-width] duration-300"
                  style={{ opacity: sidebarCollapsed ? 0 : 1, maxWidth: sidebarCollapsed ? 0 : '200px' }}
                >
                  {label}
                </span>
              </NavLink>
            ))}
          </div>
        )}
      </nav>

      {/* Collapse toggle */}
      <div className="flex-shrink-0 border-t border-white/8 p-2">
        <button
          onClick={toggleSidebarCollapse}
          title={sidebarCollapsed ? 'Expand sidebar' : 'Collapse sidebar'}
          className="flex w-full items-center justify-center gap-2 rounded-lg px-3 py-2 text-slate-400 hover:bg-white/8 hover:text-white transition-colors duration-150"
        >
          {sidebarCollapsed ? (
            <ChevronRight className="h-4 w-4" />
          ) : (
            <>
              <ChevronLeft className="h-4 w-4" />
              <span className="text-xs overflow-hidden whitespace-nowrap transition-[opacity,max-width] duration-300"
                style={{ opacity: sidebarCollapsed ? 0 : 1, maxWidth: sidebarCollapsed ? 0 : '200px' }}
              >
                Collapse
              </span>
            </>
          )}
        </button>
      </div>
    </aside>
  );
};
