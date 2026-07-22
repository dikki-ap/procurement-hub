import { useParams } from 'react-router-dom';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { RotateCcw, CheckCircle } from 'lucide-react';
import { toast } from 'sonner';
import { fulfillmentApi } from '@/features/fulfillment/api/fulfillmentApi';
import { fmtDate } from '@/shared/lib/date';

const statusCfg: Record<string, string> = {
  Created:      'bg-amber-50 text-amber-700',
  Acknowledged: 'bg-blue-50 text-blue-700',
  Shipped:      'bg-indigo-50 text-indigo-700',
  Received:     'bg-emerald-50 text-emerald-700',
};

function StatusBadge({ status }: { status: string }) {
  return (
    <span className={`inline-flex items-center px-2 py-0.5 rounded-full text-xs font-medium ${statusCfg[status] ?? 'bg-slate-100 text-slate-600'}`}>
      {status}
    </span>
  );
}

export default function VendorReturnOrdersPage() {
  const { vendorId } = useParams<{ vendorId: string }>();
  const qc           = useQueryClient();

  const { data: returns = [], isLoading } = useQuery({
    queryKey: ['vendor-portal', 'return-orders', vendorId],
    queryFn:  () => fulfillmentApi.getVendorReturnOrders(vendorId!),
    enabled:  !!vendorId,
  });

  const acknowledge = useMutation({
    mutationFn: (returnOrderId: string) => fulfillmentApi.acknowledgeReturn(vendorId!, returnOrderId),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['vendor-portal', 'return-orders', vendorId] });
      toast.success('Return order acknowledged.');
    },
    onError: () => toast.error('Failed to acknowledge return.'),
  });

  if (isLoading) return <div className="animate-pulse space-y-4"><div className="h-8 bg-slate-100 rounded w-1/3" /><div className="h-48 bg-slate-100 rounded" /></div>;

  return (
    <div>
      <div className="flex items-center gap-2 mb-6">
        <RotateCcw className="h-5 w-5 text-muted-foreground" />
        <div>
          <h1 className="text-xl sm:text-2xl font-semibold">Return Orders</h1>
          <p className="text-sm text-muted-foreground">Goods the buyer has requested to return</p>
        </div>
      </div>

      {returns.length === 0 ? (
        <div className="bg-white rounded-xl border border-slate-100 p-12 text-center">
          <RotateCcw className="h-8 w-8 text-slate-300 mx-auto mb-3" />
          <p className="text-slate-500 text-sm">No return orders.</p>
        </div>
      ) : (
        <div className="space-y-3">
          {returns.map((r) => (
            <div key={r.id} className="bg-white rounded-xl border border-slate-100 p-4">
              <div className="flex items-start justify-between gap-3">
                <div>
                  <div className="flex items-center gap-2 mb-1">
                    <span className="font-mono font-semibold text-slate-800">{r.returnNumber}</span>
                    <StatusBadge status={r.status} />
                  </div>
                  {r.reason && <p className="text-sm text-slate-600">{r.reason}</p>}
                  <p className="text-xs text-slate-400 mt-1">Created {fmtDate(r.createdAt)}</p>
                  {r.acknowledgedAt && <p className="text-xs text-slate-400">Acknowledged {fmtDate(r.acknowledgedAt)}</p>}
                  {r.shippedAt && <p className="text-xs text-slate-400">Shipped {fmtDate(r.shippedAt)}</p>}
                  {r.receivedAt && <p className="text-xs text-emerald-600">Received {fmtDate(r.receivedAt)}</p>}
                </div>
                {r.status === 'Created' && (
                  <button
                    onClick={() => acknowledge.mutate(r.id)}
                    disabled={acknowledge.isPending}
                    className="flex items-center gap-1.5 px-3 py-1.5 text-xs border border-blue-200 text-blue-600 rounded-lg hover:bg-blue-50 disabled:opacity-60"
                  >
                    <CheckCircle className="h-3.5 w-3.5" />
                    Acknowledge
                  </button>
                )}
              </div>
            </div>
          ))}
        </div>
      )}
    </div>
  );
}
