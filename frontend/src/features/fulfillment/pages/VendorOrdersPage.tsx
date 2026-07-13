import { useParams } from 'react-router-dom';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { ShoppingCart, CheckCircle } from 'lucide-react';
import { toast } from 'sonner';
import { Button } from '@/components/ui/button';
import { DataTable, type DataTableColumn } from '@/shared/components/DataTable';
import { fulfillmentApi, type POListDto, type POStatus } from '../api/fulfillmentApi';
import { extractApiError } from '@/shared/lib/apiError';
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

export default function VendorOrdersPage() {
  const { vendorId } = useParams<{ vendorId: string }>();
  const qc           = useQueryClient();

  const { data: pos = [], isLoading } = useQuery({
    queryKey: ['vendor-orders', vendorId],
    queryFn:  () => fulfillmentApi.getVendorPOs(vendorId!).then(r => r.data),
    enabled:  !!vendorId,
  });

  const ackMut = useMutation({
    mutationFn: (id: string) => fulfillmentApi.acknowledgePO(id),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['vendor-orders', vendorId] });
      toast.success('PO acknowledged');
    },
    onError: (error: unknown) => toast.error(extractApiError(error, 'Failed to acknowledge PO')),
  });

  const columns: DataTableColumn<POListDto>[] = [
    { key: 'poNumber', header: 'PO Number', sortable: true },
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
      key: 'issuedAt',
      header: 'Issued At',
      render: (row) =>
        fmtDate(row.issuedAt),
    },
  ];

  return (
    <div className="space-y-4">
      <div className="flex items-center gap-2">
        <ShoppingCart className="h-5 w-5 text-muted-foreground flex-shrink-0" />
        <div>
          <h1 className="text-xl sm:text-2xl font-semibold">Purchase Orders</h1>
          <p className="text-sm text-muted-foreground hidden sm:block">Track and acknowledge your purchase orders</p>
        </div>
      </div>

      <DataTable
        data={pos}
        columns={columns}
        isLoading={isLoading}
        searchPlaceholder="Search PO number..."
        rowActions={(row) =>
          row.status === 'Issued' ? (
            <Button
              size="sm"
              variant="outline"
              onClick={() => ackMut.mutate(row.id)}
              disabled={ackMut.isPending}
            >
              <CheckCircle className="h-3.5 w-3.5 mr-1" /> Acknowledge
            </Button>
          ) : null
        }
      />
    </div>
  );
}
