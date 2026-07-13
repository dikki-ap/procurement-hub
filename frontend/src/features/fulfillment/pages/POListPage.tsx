import { useState } from 'react';
import { useQuery } from '@tanstack/react-query';
import { useNavigate } from 'react-router-dom';
import { ShoppingCart, Plus, Eye } from 'lucide-react';
import { Button } from '@/components/ui/button';
import { DataTable, type DataTableColumn } from '@/shared/components/DataTable';
import { fulfillmentApi, type POListDto, type POStatus } from '../api/fulfillmentApi';
import { useAuthStore } from '@/stores/authStore';
import { NewPOModal } from './NewPOModal';
import { fmtDate } from '@/shared/lib/date';

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
  const navigate      = useNavigate();
  const { user }      = useAuthStore();
  const companyId     = user?.companyId ?? '';
  const [showNew, setShowNew] = useState(false);

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
        fmtDate(row.expectedDelivery),
    },
    {
      key: 'createdAt',
      header: 'Created',
      render: (row) => fmtDate(row.createdAt),
    },
  ];

  return (
    <div className="space-y-4">
      <div className="flex flex-wrap items-center justify-between gap-3">
        <div className="flex items-center gap-2">
          <ShoppingCart className="h-5 w-5 text-muted-foreground flex-shrink-0" />
          <h1 className="text-xl sm:text-2xl font-semibold">Purchase Orders</h1>
        </div>
        <Button size="sm" onClick={() => setShowNew(true)}>
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
      <NewPOModal open={showNew} onClose={() => setShowNew(false)} />
    </div>
  );
}
