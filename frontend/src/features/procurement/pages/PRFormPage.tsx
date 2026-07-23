import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useMutation, useQueryClient } from '@tanstack/react-query';
import { ArrowLeft, Plus, Trash2 } from 'lucide-react';
import { toast } from 'sonner';
import { Button } from '@/components/ui/button';
import { procurementApi, type CreatePRItemRequest } from '../api/procurementApi';
import { extractApiError } from '@/shared/lib/apiError';
import { useAuthStore } from '@/stores/authStore';

const inputCls = 'w-full rounded-lg border border-slate-200 px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500/20 focus:border-blue-400';

const emptyItem = (): CreatePRItemRequest => ({
  itemDescription: '',
  quantity: 1,
  estimatedUnitPrice: 0,
});

export default function PRFormPage() {
  const navigate   = useNavigate();
  const qc         = useQueryClient();
  const companyId  = useAuthStore(s => s.user?.companyId ?? '');
  const [items, setItems] = useState<CreatePRItemRequest[]>([emptyItem()]);

  const mutation = useMutation({
    mutationFn: procurementApi.createPR,
    onSuccess:  () => {
      qc.invalidateQueries({ queryKey: ['prs'] });
      toast.success('Purchase Requisition created');
      navigate('..');
    },
    onError: (error: unknown) => toast.error(extractApiError(error, 'Failed to create PR')),
  });

  const handleSubmit = (e: React.FormEvent<HTMLFormElement>) => {
    e.preventDefault();
    const fd = new FormData(e.currentTarget);
    mutation.mutate({
      companyId:        companyId,
      title:            fd.get('title') as string,
      description:      (fd.get('description') as string) || undefined,
      department:       fd.get('department') as string,
      deliveryLocation: (fd.get('deliveryLocation') as string) || undefined,
      requiredDate:     fd.get('requiredDate') as string,
      notes:            (fd.get('notes') as string) || undefined,
      items,
    });
  };

  const addItem    = () => setItems(prev => [...prev, emptyItem()]);
  const removeItem = (idx: number) => setItems(prev => prev.filter((_, i) => i !== idx));
  const updateItem = (idx: number, field: keyof CreatePRItemRequest, value: string | number) =>
    setItems(prev => prev.map((item, i) => i === idx ? { ...item, [field]: value } : item));

  return (
    <div className="max-w-3xl space-y-6">
      <div className="flex items-center gap-3">
        <Button variant="ghost" size="sm" onClick={() => navigate('..')}>
          <ArrowLeft className="h-4 w-4 mr-1" /> Back
        </Button>
        <h1 className="text-xl font-semibold">New Purchase Requisition</h1>
      </div>

      <form onSubmit={handleSubmit} className="space-y-6">
        {/* Header */}
        <div className="grid grid-cols-2 gap-4">
          <div className="col-span-2">
            <label className="block text-sm font-medium mb-1">Title *</label>
            <input name="title" required className={inputCls} placeholder="e.g. Office Supplies Q3 2026" />
          </div>
          <div>
            <label className="block text-sm font-medium mb-1">Department *</label>
            <input name="department" required className={inputCls} placeholder="Operations" />
          </div>
          <div>
            <label className="block text-sm font-medium mb-1">Required Date *</label>
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
          <div className="col-span-2">
            <label className="block text-sm font-medium mb-1">Notes</label>
            <textarea name="notes" rows={2} className={inputCls} placeholder="Additional notes" />
          </div>
        </div>

        {/* Items */}
        <div>
          <div className="flex items-center justify-between mb-3">
            <h2 className="text-base font-semibold">Items</h2>
            <Button type="button" size="sm" variant="outline" onClick={addItem}>
              <Plus className="h-3.5 w-3.5 mr-1" /> Add Item
            </Button>
          </div>
          <div className="space-y-3">
            {items.map((item, idx) => (
              <div key={idx} className="grid grid-cols-12 gap-2 items-end p-3 bg-muted/30 rounded-lg">
                <div className="col-span-5">
                  <label className="block text-xs font-medium mb-1">Description *</label>
                  <input required className={inputCls} value={item.itemDescription} placeholder="Item description"
                    onChange={e => updateItem(idx, 'itemDescription', e.target.value)} />
                </div>
                <div className="col-span-2">
                  <label className="block text-xs font-medium mb-1">Qty *</label>
                  <input required type="number" min="0.01" step="0.01" className={inputCls}
                    value={item.quantity}
                    onChange={e => updateItem(idx, 'quantity', parseFloat(e.target.value))} />
                </div>
                <div className="col-span-2">
                  <label className="block text-xs font-medium mb-1">Unit</label>
                  <input className={inputCls} value={item.unitLabel ?? ''} placeholder="pcs"
                    onChange={e => updateItem(idx, 'unitLabel', e.target.value)} />
                </div>
                <div className="col-span-2">
                  <label className="block text-xs font-medium mb-1">Unit Price</label>
                  <input type="number" min="0" step="1000" className={inputCls}
                    value={item.estimatedUnitPrice}
                    onChange={e => updateItem(idx, 'estimatedUnitPrice', parseFloat(e.target.value))} />
                </div>
                <div className="col-span-1 flex justify-end">
                  <Button type="button" variant="ghost" size="sm" className="text-red-500"
                    onClick={() => removeItem(idx)} disabled={items.length === 1}>
                    <Trash2 className="h-3.5 w-3.5" />
                  </Button>
                </div>
              </div>
            ))}
          </div>
        </div>

        <div className="flex gap-3 pt-2">
          <Button type="submit" disabled={mutation.isPending}>
            {mutation.isPending ? 'Creating...' : 'Create PR'}
          </Button>
          <Button type="button" variant="outline" onClick={() => navigate('..')}>
            Cancel
          </Button>
        </div>
      </form>
    </div>
  );
}
