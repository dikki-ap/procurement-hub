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
            element: (
              <div className="p-8">
                <h1 className="text-2xl font-bold">Dashboard</h1>
                <p className="text-muted-foreground mt-2">Coming soon...</p>
              </div>
            ),
          },
          {
            path: 'vendors',
            lazy: () =>
              import('@/features/vendors/pages/VendorListPage').then((m) => ({
                Component: m.default,
              })),
          },
          {
            path: 'vendors/new',
            lazy: () =>
              import('@/features/vendors/pages/VendorFormPage').then((m) => ({
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
                path: 'materials/new',
                lazy: () =>
                  import(
                    '@/features/master-data/material/pages/MaterialFormPage'
                  ).then((m) => ({ Component: m.default })),
              },
              {
                path: 'materials/:id',
                lazy: () =>
                  import(
                    '@/features/master-data/material/pages/MaterialFormPage'
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
                path: 'currencies/new',
                lazy: () =>
                  import(
                    '@/features/master-data/currency/pages/CurrencyFormPage'
                  ).then((m) => ({ Component: m.default })),
              },
              {
                path: 'currencies/:id',
                lazy: () =>
                  import(
                    '@/features/master-data/currency/pages/CurrencyFormPage'
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
                path: 'locations/new',
                lazy: () =>
                  import(
                    '@/features/master-data/location/pages/LocationFormPage'
                  ).then((m) => ({ Component: m.default })),
              },
              {
                path: 'locations/:id',
                lazy: () =>
                  import(
                    '@/features/master-data/location/pages/LocationFormPage'
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
                path: 'payment-terms/new',
                lazy: () =>
                  import(
                    '@/features/master-data/payment-term/pages/PaymentTermFormPage'
                  ).then((m) => ({ Component: m.default })),
              },
              {
                path: 'payment-terms/:id',
                lazy: () =>
                  import(
                    '@/features/master-data/payment-term/pages/PaymentTermFormPage'
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
                path: 'uoms/new',
                lazy: () =>
                  import('@/features/master-data/uom/pages/UOMFormPage').then(
                    (m) => ({ Component: m.default })
                  ),
              },
              {
                path: 'uoms/:id',
                lazy: () =>
                  import('@/features/master-data/uom/pages/UOMFormPage').then(
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
                path: 'material-categories/new',
                lazy: () =>
                  import(
                    '@/features/master-data/material-category/pages/MaterialCategoryFormPage'
                  ).then((m) => ({ Component: m.default })),
              },
              {
                path: 'material-categories/:id',
                lazy: () =>
                  import(
                    '@/features/master-data/material-category/pages/MaterialCategoryFormPage'
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
