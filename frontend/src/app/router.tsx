import { createBrowserRouter } from 'react-router-dom';
import { ProtectedRoute } from '@/features/auth/components/ProtectedRoute';
import { AppShell } from '@/shared/components/layout/AppShell';

export const router = createBrowserRouter([
  {
    path: '/login',
    lazy: () =>
      import('@/features/auth/pages/LoginRedirectPage').then((m) => ({
        Component: m.default,
      })),
  },
  {
    path: '/unauthorized',
    lazy: () =>
      import('@/features/auth/pages/UnauthorizedPage').then((m) => ({
        Component: m.default,
      })),
  },
  {
    path: '/app',
    element: (
      <ProtectedRoute
        requiredRoles={[
          'super_admin',
          'requester',
          'purchasing',
          'approver',
          'finance',
          'management',
        ]}
      />
    ),
    children: [
      {
        element: <AppShell />,
        children: [
          {
            path: 'dashboard',
            lazy: () =>
              import('@/features/analytics/pages/AnalyticsDashboardPage').then((m) => ({
                Component: m.default,
              })),
          },
          {
            path: 'vendors',
            lazy: () =>
              import('@/features/vendors/pages/VendorListPage').then((m) => ({
                Component: m.default,
              })),
          },
          {
            path: 'vendors/:id',
            lazy: () =>
              import('@/features/vendors/pages/VendorDetailPage').then((m) => ({
                Component: m.default,
              })),
          },
          // ── Approval Engine ──────────────────────────────────────────────
          {
            path: 'approval',
            element: <ProtectedRoute requiredRoles={['super_admin', 'approver', 'requester', 'purchasing', 'finance', 'management']} />,
            children: [
              {
                path: 'inbox',
                lazy: () => import('@/features/approval/pages/ApprovalInboxPage').then(m => ({ Component: m.default })),
              },
              {
                path: ':id',
                lazy: () => import('@/features/approval/pages/ApprovalDetailPage').then(m => ({ Component: m.default })),
              },
              {
                path: 'policies',
                lazy: () => import('@/features/approval/pages/ApprovalPoliciesPage').then(m => ({ Component: m.default })),
              },
              {
                path: 'approver-matrix',
                element: <ProtectedRoute requiredRoles={['super_admin']} />,
                children: [
                  {
                    path: '',
                    lazy: () => import('@/features/approval/pages/ApproverMatrixPage').then(m => ({ Component: m.default })),
                  },
                ],
              },
            ],
          },
          // ── Procurement ──────────────────────────────────────────────────
          {
            path: 'procurement',
            element: <ProtectedRoute requiredRoles={['super_admin', 'requester', 'purchasing']} />,
            children: [
              {
                path: 'prs',
                lazy: () => import('@/features/procurement/pages/PRListPage').then(m => ({ Component: m.default })),
              },
              {
                path: 'prs/:id',
                lazy: () => import('@/features/procurement/pages/PRDetailPage').then(m => ({ Component: m.default })),
              },
              {
                path: 'rfqs',
                lazy: () => import('@/features/procurement/pages/RFQListPage').then(m => ({ Component: m.default })),
              },
              {
                path: 'rfqs/:id',
                lazy: () => import('@/features/procurement/pages/RFQDetailPage').then(m => ({ Component: m.default })),
              },
              {
                path: 'rfqs/:id/evaluation',
                lazy: () => import('@/features/procurement/pages/BidEvaluationPage').then(m => ({ Component: m.default })),
              },
            ],
          },
          // ── Fulfillment ──────────────────────────────────────────────────
          {
            path: 'fulfillment',
            element: <ProtectedRoute requiredRoles={['super_admin', 'purchasing', 'finance']} />,
            children: [
              {
                path: 'purchase-orders',
                lazy: () => import('@/features/fulfillment/pages/POListPage').then(m => ({ Component: m.default })),
              },
              {
                path: 'purchase-orders/:id',
                lazy: () => import('@/features/fulfillment/pages/PODetailPage').then(m => ({ Component: m.default })),
              },
              {
                path: 'purchase-orders/:poId/grns/new',
                lazy: () => import('@/features/fulfillment/pages/GRNFormPage').then(m => ({ Component: m.default })),
              },
              {
                path: 'invoices',
                lazy: () => import('@/features/fulfillment/pages/InvoiceListPage').then(m => ({ Component: m.default })),
              },
            ],
          },
          // ── Audit Log ────────────────────────────────────────────────────
          {
            path: 'audit',
            element: <ProtectedRoute requiredRoles={['super_admin']} />,
            children: [
              {
                path: '',
                lazy: () =>
                  import('@/features/audit/pages/AuditLogPage').then((m) => ({
                    Component: m.default,
                  })),
              },
            ],
          },
          {
            path: 'master-data',
            element: <ProtectedRoute requiredRoles={['super_admin']} />,
            children: [
              {
                path: 'materials',
                lazy: () =>
                  import(
                    '@/features/master-data/material/pages/MaterialListPage'
                  ).then((m) => ({ Component: m.default })),
              },
              {
                path: 'currencies',
                lazy: () =>
                  import(
                    '@/features/master-data/currency/pages/CurrencyListPage'
                  ).then((m) => ({ Component: m.default })),
              },
              {
                path: 'locations',
                lazy: () =>
                  import(
                    '@/features/master-data/location/pages/LocationListPage'
                  ).then((m) => ({ Component: m.default })),
              },
              {
                path: 'payment-terms',
                lazy: () =>
                  import(
                    '@/features/master-data/payment-term/pages/PaymentTermListPage'
                  ).then((m) => ({ Component: m.default })),
              },
              {
                path: 'uoms',
                lazy: () =>
                  import('@/features/master-data/uom/pages/UOMListPage').then(
                    (m) => ({ Component: m.default })
                  ),
              },
              {
                path: 'material-categories',
                lazy: () =>
                  import(
                    '@/features/master-data/material-category/pages/MaterialCategoryListPage'
                  ).then((m) => ({ Component: m.default })),
              },
              {
                path: 'document-types',
                lazy: () =>
                  import(
                    '@/features/master-data/document-type/pages/DocumentTypeListPage'
                  ).then((m) => ({ Component: m.default })),
              },
            ],
          },
        ],
      },
    ],
  },
  {
    path: '/vendor-registration',
    lazy: () =>
      import('@/features/vendor-registration/pages/VendorRegistrationPage').then((m) => ({
        Component: m.default,
      })),
  },
  {
    path: '/app/vendor-portal/:vendorId',
    element: <ProtectedRoute requiredRoles={['vendor_admin', 'vendor_staff']} />,
    children: [
      {
        element: <AppShell />,
        children: [
          {
            path: 'profile',
            lazy: () =>
              import('@/features/vendor-portal/pages/VendorPortalProfilePage').then((m) => ({
                Component: m.default,
              })),
          },
          {
            path: 'documents',
            lazy: () =>
              import('@/features/vendor-portal/pages/VendorPortalDocumentsPage').then((m) => ({
                Component: m.default,
              })),
          },
          {
            path: 'bids',
            lazy: () =>
              import('@/features/vendor-portal/pages/VendorActiveBidsPage').then((m) => ({
                Component: m.default,
              })),
          },
          {
            path: 'bids/:id',
            lazy: () =>
              import('@/features/vendor-portal/pages/VendorRFQDetailPage').then((m) => ({
                Component: m.default,
              })),
          },
          {
            path: 'orders',
            lazy: () =>
              import('@/features/fulfillment/pages/VendorOrdersPage').then((m) => ({
                Component: m.default,
              })),
          },
          {
            path: 'invoices',
            lazy: () =>
              import('@/features/fulfillment/pages/VendorInvoicesPage').then((m) => ({
                Component: m.default,
              })),
          },
        ],
      },
    ],
  },
  {
    path: '/',
    lazy: () =>
      import('@/features/auth/pages/LoginRedirectPage').then((m) => ({
        Component: m.default,
      })),
  },
  {
    path: '*',
    lazy: () =>
      import('@/features/auth/pages/NotFoundPage').then((m) => ({
        Component: m.default,
      })),
  },
]);
