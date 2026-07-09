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
import { procurementApi, type CreatePRItemRequest } from '../api/procurementApi';

const COMPANY_ID = '00000000-0000-0000-0000-000000000001';

const inputCls =
  'w-full rounded-lg border border-slate-200 px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500/20 focus:border-blue-400';

const emptyItem = (): CreatePRItemRequest => ({ itemDescription: '', quantity: 1, estimatedUnitPrice: 0 });

type Props = { open: boolean; onClose: () => void };

export function NewPRModal({ open, onClose }: Props) {
  const qc = useQueryClient();
  const [items, setItems] = useState<CreatePRItemRequest[]>([emptyItem()]);

  const mutation = useMutation({
    mutationFn: procurementApi.createPR,
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['prs'] });
      toast.success('Purchase Requisition created', { duration: 3000 });
      setItems([emptyItem()]);
      onClose();
    },
    onError: () => toast.error('Failed to create PR'),
  });

  const addItem    = () => setItems(p => [...p, emptyItem()]);
  const removeItem = (i: number) => setItems(p => p.filter((_, idx) => idx !== i));
  const updateItem = (i: number, k: keyof CreatePRItemRequest, v: string | number) =>
    setItems(p => p.map((item, idx) => idx === i ? { ...item, [k]: v } : item));

  const isPending = mutation.isPending;

  const handleSubmit = (e: React.FormEvent<HTMLFormElement>) => {
    e.preventDefault();
    const fd = new FormData(e.currentTarget);
    mutation.mutate({
      companyId:        COMPANY_ID,
      title:            fd.get('title') as string,
      description:      (fd.get('description') as string) || undefined,
      department:       fd.get('department') as string,
      deliveryLocation: (fd.get('deliveryLocation') as string) || undefined,
      requiredDate:     fd.get('requiredDate') as string,
      notes:            (fd.get('notes') as string) || undefined,
      items,
    });
  };

  return (
    <Dialog open={open} onOpenChange={(v) => { if (!v && !isPending) { setItems([emptyItem()]); onClose(); } }}>
      <DialogContent className="max-w-3xl max-h-[90vh] overflow-y-auto">
        <DialogHeader>
          <DialogTitle>New Purchase Requisition</DialogTitle>
        </DialogHeader>

        <form onSubmit={handleSubmit} className="space-y-5 mt-2">
          <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
            <div className="col-span-1 sm:col-span-2">
              <label className="block text-sm font-medium mb-1">Title <span className="text-red-500">*</span></label>
              <input name="title" required className={inputCls} placeholder="e.g. Office Supplies Q3 2026" />
            </div>
            <div>
              <label className="block text-sm font-medium mb-1">Department <span className="text-red-500">*</span></label>
              <input name="department" required className={inputCls} placeholder="Operations" />
            </div>
            <div>
              <label className="block text-sm font-medium mb-1">Required Date <span className="text-red-500">*</span></label>
              <input name="requiredDate" type="date" required className={inputCls}
                min={new Date().toISOString().split('T')[0]} />
            </div>
            <div>
              <label className="block text-sm font-medium mb-1">Delivery Location</label>
              <input name="deliveryLocation" className={inputCls} placeholder="Warehouse A, Jakarta" />
            </div>
            <div>
              <label className="block text-sm font-medium mb-1">Description</label>
              <input name="description" className={inputCls} placeholder="Optional description" />
            </div>
            <div className="col-span-1 sm:col-span-2">
              <label className="block text-sm font-medium mb-1">Notes</label>
              <textarea name="notes" rows={2} className={inputCls} placeholder="Additional notes" />
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
                  {/* Qty + Unit + Unit Price + Delete */}
                  <div className="flex items-end gap-2">
                    <div className="w-20 flex-shrink-0">
                      <label className="block text-xs font-medium mb-1">Qty <span className="text-red-500">*</span></label>
                      <input required type="number" min="0.01" step="0.01" className={inputCls}
                        value={item.quantity}
                        onChange={e => updateItem(idx, 'quantity', parseFloat(e.target.value) || 0)} />
                    </div>
                    <div className="w-20 flex-shrink-0">
                      <label className="block text-xs font-medium mb-1">Unit</label>
                      <input className={inputCls} value={item.unitLabel ?? ''} placeholder="pcs"
                        onChange={e => updateItem(idx, 'unitLabel', e.target.value)} />
                    </div>
                    <div className="flex-1 min-w-0">
                      <label className="block text-xs font-medium mb-1">Unit Price</label>
                      <input type="number" min="0" step="1000" className={inputCls}
                        value={item.estimatedUnitPrice}
                        onChange={e => updateItem(idx, 'estimatedUnitPrice', parseFloat(e.target.value) || 0)} />
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
          </div>

          <div className="flex justify-end gap-3 pt-2">
            <Button type="button" variant="outline" onClick={() => { setItems([emptyItem()]); onClose(); }} disabled={isPending}>
              Cancel
            </Button>
            <Button type="submit" disabled={isPending}>
              {isPending ? 'Creating...' : 'Create PR'}
            </Button>
          </div>
        </form>
      </DialogContent>
    </Dialog>
  );
}
