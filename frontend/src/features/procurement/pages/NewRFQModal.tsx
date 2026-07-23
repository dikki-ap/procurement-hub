import { useState } from 'react';
import { useMutation, useQueryClient } from '@tanstack/react-query';
import { Plus, Trash2 } from 'lucide-react';
import { toast } from 'sonner';
import { Button } from '@/components/ui/button';
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
} from '@/components/ui/dialog';
import { procurementApi, type CreateRFQItemRequest } from '../api/procurementApi';
import { extractApiError } from '@/shared/lib/apiError';
import { useAuthStore } from '@/stores/authStore';

const inputCls =
  'w-full rounded-lg border border-slate-200 px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500/20 focus:border-blue-400';

const emptyItem = (): CreateRFQItemRequest => ({ itemDescription: '', quantity: 1 });

type Props = { open: boolean; onClose: () => void };

export function NewRFQModal({ open, onClose }: Props) {
  const qc        = useQueryClient();
  const companyId = useAuthStore(s => s.user?.companyId ?? '');
  const [items, setItems] = useState<CreateRFQItemRequest[]>([emptyItem()]);

  const mutation = useMutation({
    mutationFn: procurementApi.createRFQ,
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['rfqs'] });
      toast.success('RFQ created successfully', { duration: 3000 });
      setItems([emptyItem()]);
      onClose();
    },
    onError: (error: unknown) => toast.error(extractApiError(error, 'Failed to create RFQ')),
  });

  const addItem    = () => setItems(p => [...p, emptyItem()]);
  const removeItem = (i: number) => setItems(p => p.filter((_, idx) => idx !== i));
  const updateItem = (i: number, k: keyof CreateRFQItemRequest, v: string | number) =>
    setItems(p => p.map((item, idx) => idx === i ? { ...item, [k]: v } : item));

  const isPending = mutation.isPending;

  const tomorrow = new Date(); tomorrow.setDate(tomorrow.getDate() + 1);
  const minDate  = tomorrow.toISOString().split('T')[0];

  const handleSubmit = (e: React.FormEvent<HTMLFormElement>) => {
    e.preventDefault();
    const fd = new FormData(e.currentTarget);
    mutation.mutate({
      companyId:    companyId,
      title:        fd.get('title') as string,
      bidDeadline:  fd.get('bidDeadline') as string,
      deliveryDate: (fd.get('deliveryDate') as string) || undefined,
      notes:        (fd.get('notes') as string) || undefined,
      terms:        (fd.get('terms') as string) || undefined,
      items,
    });
  };

  return (
    <Dialog open={open} onOpenChange={(v) => { if (!v && !isPending) { setItems([emptyItem()]); onClose(); } }}>
      <DialogContent className="max-w-3xl max-h-[90vh] overflow-y-auto">
        <DialogHeader>
          <DialogTitle>New Request for Quotation</DialogTitle>
        </DialogHeader>

        <form onSubmit={handleSubmit} className="space-y-5 mt-2">
          <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
            <div className="col-span-1 sm:col-span-2">
              <label className="block text-sm font-medium mb-1">Title <span className="text-red-500">*</span></label>
              <input name="title" required className={inputCls} placeholder="e.g. Procurement of Office Equipment" />
            </div>
            <div>
              <label className="block text-sm font-medium mb-1">Bid Deadline <span className="text-red-500">*</span></label>
              <input name="bidDeadline" type="datetime-local" required className={inputCls} min={minDate} />
            </div>
            <div>
              <label className="block text-sm font-medium mb-1">Delivery Date</label>
              <input name="deliveryDate" type="date" className={inputCls} min={minDate} />
            </div>
            <div>
              <label className="block text-sm font-medium mb-1">Notes</label>
              <textarea name="notes" rows={2} className={inputCls} placeholder="Optional notes" />
            </div>
            <div>
              <label className="block text-sm font-medium mb-1">Terms &amp; Conditions</label>
              <textarea name="terms" rows={2} className={inputCls} placeholder="Payment terms, delivery conditions..." />
            </div>
          </div>

          <div>
            <div className="flex items-center justify-between mb-3">
              <h2 className="text-base font-semibold">Items</h2>
              <Button type="button" size="sm" variant="outline" onClick={addItem}>
                <Plus className="h-3.5 w-3.5 mr-1" /> Add Item
              </Button>
            </div>
            <div className="space-y-2">
              {items.map((item, idx) => (
                <div key={idx} className="p-3 bg-muted/30 rounded-lg space-y-2">
                  {/* Description — full width */}
                  <div>
                    <label className="block text-xs font-medium mb-1">Description <span className="text-red-500">*</span></label>
                    <input required className={inputCls} value={item.itemDescription} placeholder="Item description"
                      onChange={e => updateItem(idx, 'itemDescription', e.target.value)} />
                  </div>
                  {/* Qty + Unit + Delete */}
                  <div className="flex items-end gap-2">
                    <div className="w-24 flex-shrink-0">
                      <label className="block text-xs font-medium mb-1">Qty <span className="text-red-500">*</span></label>
                      <input required type="number" min="0.01" step="0.01" className={inputCls}
                        value={item.quantity}
                        onChange={e => updateItem(idx, 'quantity', parseFloat(e.target.value) || 0)} />
                    </div>
                    <div className="flex-1 min-w-0">
                      <label className="block text-xs font-medium mb-1">Unit</label>
                      <input className={inputCls} value={item.unitLabel ?? ''} placeholder="pcs"
                        onChange={e => updateItem(idx, 'unitLabel', e.target.value)} />
                    </div>
                    <div className="flex items-end pb-0.5">
                      <Button type="button" variant="ghost" size="sm" className="text-red-500"
                        onClick={() => removeItem(idx)} disabled={items.length === 1}>
                        <Trash2 className="h-3.5 w-3.5" />
                      </Button>
                    </div>
                  </div>
                </div>
              ))}
            </div>
            <p className="text-xs text-muted-foreground mt-2">
              After creating, invite at least 3 vendors to open the RFQ for bidding.
            </p>
          </div>

          <div className="flex justify-end gap-3 pt-2">
            <Button type="button" variant="outline" onClick={() => { setItems([emptyItem()]); onClose(); }} disabled={isPending}>
              Cancel
            </Button>
            <Button type="submit" disabled={isPending}>
              {isPending ? 'Creating...' : 'Create RFQ'}
            </Button>
          </div>
        </form>
      </DialogContent>
    </Dialog>
  );
}
