import { useQuery } from '@tanstack/react-query';
import { useNavigate } from 'react-router-dom';
import { ShoppingCart, Plus, Eye } from 'lucide-react';
import { Button } from '@/components/ui/button';
import { DataTable, type DataTableColumn } from '@/shared/components/DataTable';
import { fulfillmentApi, type POListDto, type POStatus } from '../api/fulfillmentApi';
import { useAuthStore } from '@/stores/authStore';

const statusColor: Record<POStatus, string> = {
  Draft:           'bg-gray-100 text-gray-600',
  PendingApproval: 'bg-yellow-50 text-yellow-700',
  Approved:        'bg-blue-50 text-blue-700',
  Issued:          'bg-indigo-50 text-indigo-700',
  Acknowledged:    'bg-purple-50 text-purple-700',
  InDelivery:      'bg-orange-50 text-orange-700',
  Completed:       'bg-emerald-50 text-emerald-700',
  Cancelled:       'bg-red-50 text-red-600',
};

const fmt = (n: number) =>
  new Intl.NumberFormat('id-ID', { style: 'decimal', minimumFractionDigits: 0 }).format(n);

export default function POListPage() {
  const navigate  = useNavigate();
  const { user }  = useAuthStore();
  const companyId = user?.companyId ?? '';

  const { data: pos = [], isLoading } = useQuery({
    queryKey: ['purchase-orders', companyId],
    queryFn:  () => fulfillmentApi.getPOList(companyId),
    enabled:  !!companyId,
  });

  const columns: DataTableColumn<POListDto>[] = [
    { key: 'poNumber',   header: 'PO Number',  sortable: true },
    { key: 'vendorName', header: 'Vendor',      sortable: true },
    {
      key: 'status',
      header: 'Status',
      render: (row) => (
        <span className={`inline-flex px-2 py-0.5 rounded-full text-xs font-medium ${statusColor[row.status]}`}>
          {row.status}
        </span>
      ),
    },
    {
      key: 'totalAmount',
      header: 'Total (Rp)',
      render: (row) => fmt(row.totalAmount),
    },
    {
      key: 'expectedDelivery',
      header: 'Expected Delivery',
      render: (row) =>
        row.expectedDelivery ? new Date(row.expectedDelivery).toLocaleDateString('id-ID') : '—',
    },
    {
      key: 'createdAt',
      header: 'Created',
      render: (row) => new Date(row.createdAt).toLocaleDateString('id-ID'),
    },
  ];

  return (
    <div className="space-y-4">
      <div className="flex items-center gap-3">
        <ShoppingCart className="h-5 w-5 text-muted-foreground" />
        <h1 className="text-2xl font-semibold">Purchase Orders</h1>
        <Button size="sm" className="ml-auto" onClick={() => navigate('new')}>
          <Plus className="h-4 w-4 mr-1" /> New PO
        </Button>
      </div>

      <DataTable
        data={pos}
        columns={columns}
        isLoading={isLoading}
        searchPlaceholder="Search PO number or vendor..."
        rowActions={(row) => (
          <Button size="sm" variant="ghost" onClick={() => navigate(row.id)}>
            <Eye className="h-3.5 w-3.5" />
          </Button>
        )}
      />
    </div>
  );
}
