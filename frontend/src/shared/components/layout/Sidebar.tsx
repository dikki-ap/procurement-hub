import { useEffect, useState } from 'react';
import { NavLink, useLocation } from 'react-router-dom';
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
  ClipboardList,
  FileText,
  Inbox,
  Shield,
  ShoppingCart,
  Receipt,
  Building2,
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
  { to: '/app/master-data/document-types', icon: FileText, label: 'Document Types' },
];

const navCls = (collapsed: boolean) => ({ isActive }: { isActive: boolean }) =>
  `flex items-center gap-3 py-2.5 rounded-lg mx-2 text-sm transition-all duration-150 ${
    collapsed ? 'px-[18px] justify-center' : 'px-3'
  } ${
    isActive
      ? 'border-l-2 border-indigo-400 bg-indigo-500/10 text-white pl-[10px]'
      : 'border-l-2 border-transparent text-slate-400 hover:bg-white/8 hover:text-white'
  }`;

const NavLabel = ({ collapsed, children }: { collapsed: boolean; children: React.ReactNode }) => (
  <span
    className="whitespace-nowrap overflow-hidden transition-[opacity,max-width] duration-300"
    style={{ opacity: collapsed ? 0 : 1, maxWidth: collapsed ? 0 : '200px' }}
  >
    {children}
  </span>
);

export const Sidebar = () => {
  const { sidebarCollapsed, toggleSidebarCollapse, sidebarOpen, setSidebarOpen } = useUIStore();
  const { user } = useAuthStore();
  const location = useLocation();

  // Always full-width on mobile — collapse state only applies on desktop
  const [isMobile, setIsMobile] = useState(() => window.innerWidth < 768);
  useEffect(() => {
    const handler = () => setIsMobile(window.innerWidth < 768);
    window.addEventListener('resize', handler);
    return () => window.removeEventListener('resize', handler);
  }, []);

  const ec = !isMobile && sidebarCollapsed; // effectiveCollapsed

  // Auto-close drawer when navigating on mobile
  useEffect(() => {
    if (isMobile) setSidebarOpen(false);
  }, [location.pathname, isMobile, setSidebarOpen]);

  const isSuperAdmin = user?.roles?.includes('super_admin') ?? false;
  const isPurchasing = user?.roles?.some(r => ['purchasing', 'super_admin'].includes(r)) ?? false;
  const isFinance    = user?.roles?.some(r => ['finance',    'super_admin'].includes(r)) ?? false;
  const isVendor     = user?.roles?.some(r => ['vendor_admin', 'vendor_staff'].includes(r)) ?? false;

  const vendorMatch = location.pathname.match(/\/app\/vendor-portal\/([^/]+)/);
  const vendorId    = vendorMatch?.[1];

  return (
    <aside
      className={[
        'fixed md:static inset-y-0 left-0',
        'z-50 md:z-auto',
        'flex flex-col flex-shrink-0',
        'bg-[#0f1729] text-slate-200',
        'transition-all duration-300 ease-in-out',
        sidebarOpen ? 'translate-x-0' : '-translate-x-full md:translate-x-0',
      ].join(' ')}
      style={{ width: ec ? '64px' : '240px' }}
    >
      {/* Logo */}
      <div className="flex items-center gap-3 h-14 px-3 flex-shrink-0 border-b border-white/8">
        <img src="/favicon.svg" alt="Procurement Hub" className="flex-shrink-0 w-8 h-8 rounded-lg" />
        <span
          className="font-semibold text-sm text-white whitespace-nowrap transition-[opacity,width] duration-300 overflow-hidden"
          style={{ opacity: ec ? 0 : 1, width: ec ? 0 : 'auto' }}
        >
          Procurement Hub
        </span>
      </div>

      {/* Navigation */}
      <nav className="flex-1 overflow-y-auto overflow-x-hidden py-3 space-y-0.5">

        {/* ── Vendor Portal ── */}
        {isVendor && vendorId && (
          <>
            <NavLink to={`/app/vendor-portal/${vendorId}/profile`} title="Company Profile" className={navCls(ec)}>
              <Building2 className="h-4 w-4 flex-shrink-0" />
              <NavLabel collapsed={ec}>Company Profile</NavLabel>
            </NavLink>
            <div className="pt-1">
              <NavLink to={`/app/vendor-portal/${vendorId}/documents`} title="Documents" className={navCls(ec)}>
                <FileText className="h-4 w-4 flex-shrink-0" />
                <NavLabel collapsed={ec}>Documents</NavLabel>
              </NavLink>
            </div>
            <div className="pt-1">
              <NavLink to={`/app/vendor-portal/${vendorId}/orders`} title="Purchase Orders" className={navCls(ec)}>
                <ShoppingCart className="h-4 w-4 flex-shrink-0" />
                <NavLabel collapsed={ec}>Purchase Orders</NavLabel>
              </NavLink>
            </div>
            <div className="pt-1">
              <NavLink to={`/app/vendor-portal/${vendorId}/invoices`} title="My Invoices" className={navCls(ec)}>
                <Receipt className="h-4 w-4 flex-shrink-0" />
                <NavLabel collapsed={ec}>My Invoices</NavLabel>
              </NavLink>
            </div>
          </>
        )}

        {/* ── Internal Portal ── */}
        {!isVendor && (
          <>
            <NavLink to="/app/dashboard" title="Dashboard" className={navCls(ec)}>
              <LayoutDashboard className="h-4 w-4 flex-shrink-0" />
              <NavLabel collapsed={ec}>Dashboard</NavLabel>
            </NavLink>

            <div className="pt-3">
              <NavLink to="/app/procurement/prs" title="Purchase Requisitions" className={navCls(ec)}>
                <ClipboardList className="h-4 w-4 flex-shrink-0" />
                <NavLabel collapsed={ec}>Purchase Requisitions</NavLabel>
              </NavLink>
            </div>

            <div className="pt-1">
              <NavLink to="/app/procurement/rfqs" title="RFQs" className={navCls(ec)}>
                <FileText className="h-4 w-4 flex-shrink-0" />
                <NavLabel collapsed={ec}>RFQs</NavLabel>
              </NavLink>
            </div>

            {(isPurchasing || isFinance) && (
              <div className="pt-1">
                <NavLink to="/app/fulfillment/purchase-orders" title="Purchase Orders" className={navCls(ec)}>
                  <ShoppingCart className="h-4 w-4 flex-shrink-0" />
                  <NavLabel collapsed={ec}>Purchase Orders</NavLabel>
                </NavLink>
              </div>
            )}

            {isFinance && (
              <div className="pt-1">
                <NavLink to="/app/fulfillment/invoices" title="Invoices" className={navCls(ec)}>
                  <Receipt className="h-4 w-4 flex-shrink-0" />
                  <NavLabel collapsed={ec}>Invoices</NavLabel>
                </NavLink>
              </div>
            )}

            <div className="pt-3">
              <NavLink to="/app/approval/inbox" title="Approval Inbox" className={navCls(ec)}>
                <Inbox className="h-4 w-4 flex-shrink-0" />
                <NavLabel collapsed={ec}>Approval Inbox</NavLabel>
              </NavLink>
            </div>

            <div className="pt-3">
              <NavLink to="/app/vendors" title="Vendors" className={navCls(ec)}>
                <Users className="h-4 w-4 flex-shrink-0" />
                <NavLabel collapsed={ec}>Vendors</NavLabel>
              </NavLink>
            </div>

            {isSuperAdmin && (
              <div className="pt-3">
                <NavLink to="/app/approval/policies" title="Approval Policies" className={navCls(ec)}>
                  <Shield className="h-4 w-4 flex-shrink-0" />
                  <NavLabel collapsed={ec}>Approval Policies</NavLabel>
                </NavLink>

                <div
                  className="overflow-hidden transition-[max-height,opacity] duration-300 mt-3"
                  style={{ maxHeight: ec ? 0 : '40px', opacity: ec ? 0 : 1 }}
                >
                  <p className="px-5 pb-1 text-[10px] font-semibold text-slate-500 uppercase tracking-widest">
                    Master Data
                  </p>
                </div>

                {masterDataLinks.map(({ to, icon: Icon, label }) => (
                  <NavLink key={to} to={to} title={label} className={navCls(ec)}>
                    <Icon className="h-4 w-4 flex-shrink-0" />
                    <NavLabel collapsed={ec}>{label}</NavLabel>
                  </NavLink>
                ))}
              </div>
            )}
          </>
        )}
      </nav>

      {/* Collapse toggle — desktop only */}
      <div className="hidden md:flex flex-shrink-0 border-t border-white/8 p-2">
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
              <span className="text-xs overflow-hidden whitespace-nowrap">Collapse</span>
            </>
          )}
        </button>
      </div>
    </aside>
  );
};
