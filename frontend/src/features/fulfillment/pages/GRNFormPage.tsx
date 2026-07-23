import { useState } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { ArrowLeft } from 'lucide-react';
import { toast } from 'sonner';
import { Button } from '@/components/ui/button';
import { fulfillmentApi, type QualityStatus, type CreateGRNPayload } from '../api/fulfillmentApi';
import { useAuthStore } from '@/stores/authStore';

const inputCls =
  'w-full rounded-lg border border-slate-200 px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500/20 focus:border-blue-400';

type ItemRow = {
  poItemId:        string;
  description:     string;
  orderedQty:      number;
  alreadyReceived: number;
  receivedQty:     number;
  rejectedQty:     number;
  qualityStatus:   QualityStatus;
  rejectReason:    string;
};

export default function GRNFormPage() {
  const { poId }  = useParams<{ poId: string }>();
  const navigate  = useNavigate();
  const qc        = useQueryClient();
  const { user }  = useAuthStore();

  const [rows, setRows] = useState<ItemRow[]>([]);

  const { data: po, isLoading } = useQuery({
    queryKey: ['purchase-order', poId],
    queryFn:  () => fulfillmentApi.getPOById(poId!).then(r => r.data),
    enabled:  !!poId,
    select: (data) => {
      if (rows.length === 0 && data) {
        setRows(
          data.items.map(item => ({
            poItemId:        item.id,
            description:     item.description,
            orderedQty:      item.quantity,
            alreadyReceived: item.receivedQty,
            receivedQty:     Math.max(0, item.quantity - item.receivedQty),
            rejectedQty:     0,
            qualityStatus:   'Accepted',
            rejectReason:    '',
          }))
        );
      }
      return data;
    },
  });

  const setRow = (idx: number, patch: Partial<ItemRow>) =>
    setRows(prev => prev.map((r, i) => (i === idx ? { ...r, ...patch } : r)));

  const mutation = useMutation({
    mutationFn: (payload: CreateGRNPayload) => fulfillmentApi.createGRN(payload),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['grns', poId] });
      qc.invalidateQueries({ queryKey: ['purchase-order', poId] });
      toast.success('GRN created');
      navigate(-1);
    },
    onError: (err: any) => {
      const msg = err?.response?.data?.message ?? 'Failed to create GRN';
      toast.error(msg);
    },
  });

  const handleSubmit = (e: React.FormEvent<HTMLFormElement>) => {
    e.preventDefault();
    const fd = new FormData(e.currentTarget);
    mutation.mutate({
      poId:           poId!,
      receivedBy:     user?.id ?? '',
      deliveryNoteNo: (fd.get('deliveryNoteNo') as string) || undefined,
      notes:          (fd.get('notes') as string) || undefined,
      items: rows.map(r => ({
        poItemId:      r.poItemId,
        receivedQty:   r.receivedQty,
        rejectedQty:   r.rejectedQty,
        qualityStatus: r.qualityStatus,
        rejectReason:  r.rejectReason || undefined,
      })),
    });
  };

  if (isLoading) return <div className="p-6 text-muted-foreground">Loading PO...</div>;
  if (!po)       return <div className="p-6 text-red-500">PO not found.</div>;

  return (
    <div className="max-w-4xl space-y-6">
      <div className="flex items-center gap-3">
        <Button variant="ghost" size="sm" onClick={() => navigate(-1)}>
          <ArrowLeft className="h-4 w-4 mr-1" /> Back
        </Button>
        <div>
          <h1 className="text-xl font-semibold">New Goods Receipt</h1>
          <p className="text-sm text-muted-foreground">
            PO: {po.poNumber} — {po.vendorName}
          </p>
        </div>
      </div>

      <form onSubmit={handleSubmit} className="space-y-6">
        <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
          <div>
            <label className="block text-sm font-medium mb-1">Delivery Note No.</label>
            <input name="deliveryNoteNo" className={inputCls} placeholder="Optional delivery note number" />
          </div>
          <div>
            <label className="block text-sm font-medium mb-1">Notes</label>
            <input name="notes" className={inputCls} placeholder="Optional notes" />
          </div>
        </div>

        <div>
          <h2 className="text-base font-semibold mb-3">Items</h2>
          {rows.length === 0 && (
            <p className="text-sm text-muted-foreground">Loading items...</p>
          )}
          <div className="space-y-3">
            {rows.map((row, idx) => {
              const remaining = row.orderedQty - row.alreadyReceived;
              return (
                <div key={row.poItemId} className="p-3 bg-muted/30 rounded-lg space-y-2">
                  <div className="flex items-center justify-between">
                    <span className="font-medium text-sm">{row.description}</span>
                    <span className="text-xs text-muted-foreground">
                      Ordered: {row.orderedQty} | Already received: {row.alreadyReceived} | Remaining: {remaining}
                    </span>
                  </div>
                  <div className="grid grid-cols-2 sm:grid-cols-4 gap-2">
                    <div>
                      <label className="block text-xs font-medium mb-1">Received Qty</label>
                      <input type="number" min="0" max={remaining} step="0.01" className={inputCls}
                        value={row.receivedQty}
                        onChange={e => setRow(idx, { receivedQty: parseFloat(e.target.value) || 0 })} />
                    </div>
                    <div>
                      <label className="block text-xs font-medium mb-1">Rejected Qty</label>
                      <input type="number" min="0" step="0.01" className={inputCls}
                        value={row.rejectedQty}
                        onChange={e => setRow(idx, { rejectedQty: parseFloat(e.target.value) || 0 })} />
                    </div>
                    <div>
                      <label className="block text-xs font-medium mb-1">Quality Status</label>
                      <select className={inputCls} value={row.qualityStatus}
                        onChange={e => setRow(idx, { qualityStatus: e.target.value as QualityStatus })}>
                        <option value="Accepted">Accepted</option>
                        <option value="Partial">Partial</option>
                        <option value="Rejected">Rejected</option>
                      </select>
                    </div>
                    <div>
                      <label className="block text-xs font-medium mb-1">Reject Reason</label>
                      <input className={inputCls} value={row.rejectReason}
                        placeholder="If rejected..."
                        onChange={e => setRow(idx, { rejectReason: e.target.value })} />
                    </div>
                  </div>
                </div>
              );
            })}
          </div>
        </div>

        <div className="flex gap-3 pt-2">
          <Button type="submit" disabled={mutation.isPending || rows.length === 0}>
            {mutation.isPending ? 'Saving...' : 'Create GRN'}
          </Button>
          <Button type="button" variant="outline" onClick={() => navigate(-1)}>
            Cancel
          </Button>
        </div>
      </form>
    </div>
  );
}
