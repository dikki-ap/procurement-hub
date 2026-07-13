import { useState } from 'react';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { Plus, Trash2 } from 'lucide-react';
import { toast } from 'sonner';
import { Button } from '@/components/ui/button';
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
} from '@/components/ui/dialog';
import { fulfillmentApi, type CreatePOPayload } from '../api/fulfillmentApi';
import { vendorApi } from '@/features/vendors/api/vendorApi';
import { extractApiError } from '@/shared/lib/apiError';
import { useBaseCurrency } from '@/shared/hooks/useBaseCurrency';
import { SearchableSelect } from '@/shared/components/SearchableSelect';

const COMPANY_ID = '00000000-0000-0000-0000-000000000001';

const inputCls =
  'w-full rounded-lg border border-slate-200 px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500/20 focus:border-blue-400';

type ItemRow = { description: string; quantity: number; unitPrice: number };
const emptyItem = (): ItemRow => ({ description: '', quantity: 1, unitPrice: 0 });

const fmt = (n: number) =>
  new Intl.NumberFormat('id-ID', { style: 'decimal', minimumFractionDigits: 0 }).format(n);

type Props = {
  open: boolean;
  onClose: () => void;
};

export function NewPOModal({ open, onClose }: Props) {
  const qc   = useQueryClient();
  const base = useBaseCurrency();
  const sym       = base?.symbol ?? base?.code ?? '?';

  const [items, setItems]       = useState<ItemRow[]>([emptyItem()]);
  const [vendorId, setVendorId] = useState('');

  const { data: allVendors = [] } = useQuery({
    queryKey: ['vendors', COMPANY_ID],
    queryFn:  () => vendorApi.getAll(COMPANY_ID),
  });
  const vendors = allVendors.filter(v => v.status === 'Active');

  const mutation = useMutation({
    mutationFn: (payload: CreatePOPayload) => fulfillmentApi.createPO(payload),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['purchase-orders'] });
      toast.success('Purchase Order created', { duration: 3000 });
      setItems([emptyItem()]);
      setVendorId('');
      onClose();
    },
    onError: (error: unknown) => toast.error(extractApiError(error, 'Failed to create PO')),
  });

  const addItem    = () => setItems(p => [...p, emptyItem()]);
  const removeItem = (i: number) => setItems(p => p.filter((_, idx) => idx !== i));
  const updateItem = (i: number, k: keyof ItemRow, v: string | number) =>
    setItems(p => p.map((item, idx) => (idx === i ? { ...item, [k]: v } : item)));

  const total = items.reduce((s, r) => s + r.quantity * r.unitPrice, 0);

  const handleSubmit = (e: React.FormEvent<HTMLFormElement>) => {
    e.preventDefault();
    const fd = new FormData(e.currentTarget);
    mutation.mutate({
      companyId:        COMPANY_ID,
      vendorId:         fd.get('vendorId') as string,
      expectedDelivery: (fd.get('expectedDelivery') as string) || undefined,
      notes:            (fd.get('notes') as string) || undefined,
      termsConditions:  (fd.get('termsConditions') as string) || undefined,
      items: items.map(i => ({
        description: i.description,
        quantity:    i.quantity,
        unitPrice:   i.unitPrice,
      })),
    });
  };

  const isPending = mutation.isPending;

  return (
    <Dialog open={open} onOpenChange={(v) => { if (!v && !isPending) { setItems([emptyItem()]); setVendorId(''); onClose(); } }}>
      <DialogContent className="max-w-3xl max-h-[90vh] overflow-y-auto">
        <DialogHeader>
          <DialogTitle>New Purchase Order</DialogTitle>
        </DialogHeader>

        <form onSubmit={handleSubmit} className="space-y-5 mt-2">
          <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
            <div className="col-span-1 sm:col-span-2">
              <label className="block text-sm font-medium mb-1">Vendor <span className="text-red-500">*</span></label>
              <input type="hidden" name="vendorId" value={vendorId} />
              <SearchableSelect
                options={vendors.map(v => ({ value: v.id, label: `${v.tradeName ?? v.legalName} (${v.vendorCode})` }))}
                value={vendorId}
                onChange={setVendorId}
                placeholder="Select vendor..."
                className={inputCls}
              />
            </div>
            <div>
              <label className="block text-sm font-medium mb-1">Expected Delivery</label>
              <input name="expectedDelivery" type="date" className={inputCls} />
            </div>
            <div>
              <label className="block text-sm font-medium mb-1">Notes</label>
              <textarea name="notes" rows={2} className={inputCls} placeholder="Optional notes" />
            </div>
            <div className="col-span-1 sm:col-span-2">
              <label className="block text-sm font-medium mb-1">Terms &amp; Conditions</label>
              <textarea name="termsConditions" rows={2} className={inputCls}
                placeholder="Payment terms, delivery conditions..." />
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
                    <input required className={inputCls} value={item.description}
                      placeholder="Item description"
                      onChange={e => updateItem(idx, 'description', e.target.value)} />
                  </div>
                  {/* Qty + Unit Price + Delete */}
                  <div className="flex items-end gap-2">
                    <div className="w-24 flex-shrink-0">
                      <label className="block text-xs font-medium mb-1">Qty <span className="text-red-500">*</span></label>
                      <input required type="number" min="0.01" step="0.01" className={inputCls}
                        value={item.quantity}
                        onChange={e => updateItem(idx, 'quantity', parseFloat(e.target.value) || 0)} />
                    </div>
                    <div className="flex-1 min-w-0">
                      <label className="block text-xs font-medium mb-1">Unit Price ({sym}) <span className="text-red-500">*</span></label>
                      <input required type="number" min="0" step="1" className={inputCls}
                        value={item.unitPrice}
                        onChange={e => updateItem(idx, 'unitPrice', parseFloat(e.target.value) || 0)} />
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
            <div className="text-right text-sm font-semibold mt-2 text-slate-700">
              Total: {sym} {fmt(total)}
            </div>
          </div>

          <div className="flex justify-end gap-3 pt-2">
            <Button type="button" variant="outline" onClick={() => { setItems([emptyItem()]); onClose(); }} disabled={isPending}>
              Cancel
            </Button>
            <Button type="submit" disabled={isPending}>
              {isPending ? 'Creating...' : 'Create PO'}
            </Button>
          </div>
        </form>
      </DialogContent>
    </Dialog>
  );
}
