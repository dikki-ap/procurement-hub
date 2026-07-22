import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { RotateCcw, CheckCircle } from 'lucide-react';
import { toast } from 'sonner';
import { fulfillmentApi, type ReturnOrderListDto } from '../api/fulfillmentApi';
import { useAuthStore } from '@/stores/authStore';
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

export default function ReturnOrdersPage() {
  const { user }   = useAuthStore();
  const qc         = useQueryClient();
  const companyId  = user?.companyId ?? '';

  const { data: returns = [], isLoading } = useQuery({
    queryKey: ['return-orders', companyId],
    queryFn:  () => fulfillmentApi.getReturnOrders(companyId),
    enabled:  !!companyId,
  });

  const confirmReceived = useMutation({
    mutationFn: (id: string) => fulfillmentApi.confirmReturnReceived(id),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['return-orders'] });
      toast.success('Return confirmed as received.');
    },
    onError: () => toast.error('Failed to confirm return.'),
  });

  if (isLoading) return <div className="animate-pulse space-y-4"><div className="h-8 bg-slate-100 rounded w-1/3" /><div className="h-48 bg-slate-100 rounded" /></div>;

  return (
    <div>
      <div className="flex items-center gap-2 mb-6">
        <RotateCcw className="h-5 w-5 text-muted-foreground" />
        <div>
          <h1 className="text-xl sm:text-2xl font-semibold">Return Orders</h1>
          <p className="text-sm text-muted-foreground">Track goods returned to vendors</p>
        </div>
      </div>

      {returns.length === 0 ? (
        <div className="bg-white rounded-xl border border-slate-100 p-12 text-center">
          <RotateCcw className="h-8 w-8 text-slate-300 mx-auto mb-3" />
          <p className="text-slate-500 text-sm">No return orders yet.</p>
        </div>
      ) : (
        <div className="bg-white rounded-xl border border-slate-100 overflow-hidden">
          <table className="w-full text-sm">
            <thead>
              <tr className="border-b border-slate-100 bg-slate-50">
                <th className="text-left px-4 py-3 font-medium text-slate-600 text-xs">Return #</th>
                <th className="text-left px-4 py-3 font-medium text-slate-600 text-xs">Vendor</th>
                <th className="text-left px-4 py-3 font-medium text-slate-600 text-xs">Status</th>
                <th className="text-left px-4 py-3 font-medium text-slate-600 text-xs">Reason</th>
                <th className="text-left px-4 py-3 font-medium text-slate-600 text-xs">Created</th>
                <th className="text-left px-4 py-3 font-medium text-slate-600 text-xs">Actions</th>
              </tr>
            </thead>
            <tbody>
              {returns.map((r) => (
                <tr key={r.id} className="border-b border-slate-50 hover:bg-slate-50/50">
                  <td className="px-4 py-3 font-mono font-medium text-slate-800">{r.returnNumber}</td>
                  <td className="px-4 py-3 text-slate-600">{r.vendorName ?? '—'}</td>
                  <td className="px-4 py-3"><StatusBadge status={r.status} /></td>
                  <td className="px-4 py-3 text-slate-500 max-w-[200px] truncate">{r.reason ?? '—'}</td>
                  <td className="px-4 py-3 text-slate-400">{fmtDate(r.createdAt)}</td>
                  <td className="px-4 py-3">
                    {r.status === 'Shipped' && (
                      <button
                        onClick={() => confirmReceived.mutate(r.id)}
                        disabled={confirmReceived.isPending}
                        className="flex items-center gap-1 text-xs text-emerald-600 hover:text-emerald-700 font-medium"
                      >
                        <CheckCircle className="h-3.5 w-3.5" />
                        Confirm Received
                      </button>
                    )}
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
    </div>
  );
}
