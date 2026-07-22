import { useState } from 'react';
import { useQuery, useMutation } from '@tanstack/react-query';
import { toast } from 'sonner';
import {
  Dialog, DialogContent, DialogHeader, DialogTitle,
} from '@/components/ui/dialog';
import { Button } from '@/components/ui/button';
import { useAuthStore } from '@/stores/authStore';
import { contractApi, type CreateContractRequest } from '../api/contractApi';
import { vendorApi } from '@/features/vendors/api/vendorApi';
import { extractApiError } from '@/shared/lib/apiError';

const inputCls =
  'w-full rounded-md border border-input bg-transparent px-3 py-2 text-sm shadow-sm transition-colors placeholder:text-muted-foreground focus-visible:outline-none focus-visible:ring-1 focus-visible:ring-ring disabled:cursor-not-allowed disabled:opacity-50';

interface Props {
  open: boolean;
  onClose: () => void;
  onCreated: (id: string) => void;
}

const emptyForm = (): CreateContractRequest => ({
  vendorId:   '',
  title:      '',
  poId:       null,
  startDate:  null,
  endDate:    null,
  value:      null,
  currencyId: null,
  notes:      null,
});

export function NewContractModal({ open, onClose, onCreated }: Props) {
  const companyId = useAuthStore(s => s.user?.companyId ?? '');
  const [form, setForm] = useState<CreateContractRequest>(emptyForm());

  const { data: vendors = [] } = useQuery({
    queryKey: ['vendors', companyId],
    queryFn: () => vendorApi.getAll(companyId),
    enabled: open && !!companyId,
    select: (data) => data.filter(v => v.status === 'Active'),
  });

  const createMut = useMutation({
    mutationFn: () => contractApi.create(form),
    onSuccess: (id) => {
      toast.success('Contract created');
      setForm(emptyForm());
      onCreated(id);
    },
    onError: (e: unknown) => toast.error(extractApiError(e, 'Failed to create contract')),
  });

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    if (!form.vendorId) { toast.error('Select a vendor'); return; }
    createMut.mutate();
  };

  const handleClose = () => {
    setForm(emptyForm());
    onClose();
  };

  return (
    <Dialog open={open} onOpenChange={(v) => { if (!v) handleClose(); }}>
      <DialogContent className="max-w-lg">
        <DialogHeader>
          <DialogTitle>New Contract</DialogTitle>
        </DialogHeader>
        <form onSubmit={handleSubmit} className="space-y-4 mt-2">
          <div>
            <label className="block text-sm font-medium mb-1">
              Vendor <span className="text-red-500">*</span>
            </label>
            <select
              required
              className={inputCls}
              value={form.vendorId}
              onChange={(e) => setForm(f => ({ ...f, vendorId: e.target.value }))}
            >
              <option value="">— Select vendor —</option>
              {vendors.map(v => (
                <option key={v.id} value={v.id}>{v.legalName}</option>
              ))}
            </select>
          </div>

          <div>
            <label className="block text-sm font-medium mb-1">
              Title <span className="text-red-500">*</span>
            </label>
            <input
              required
              className={inputCls}
              value={form.title}
              onChange={(e) => setForm(f => ({ ...f, title: e.target.value }))}
              placeholder="e.g. Supply Agreement 2026"
            />
          </div>

          <div className="grid grid-cols-2 gap-4">
            <div>
              <label className="block text-sm font-medium mb-1">Start Date</label>
              <input
                type="date"
                className={inputCls}
                value={form.startDate ?? ''}
                onChange={(e) => setForm(f => ({ ...f, startDate: e.target.value || null }))}
              />
            </div>
            <div>
              <label className="block text-sm font-medium mb-1">End Date</label>
              <input
                type="date"
                className={inputCls}
                value={form.endDate ?? ''}
                onChange={(e) => setForm(f => ({ ...f, endDate: e.target.value || null }))}
              />
            </div>
          </div>

          <div>
            <label className="block text-sm font-medium mb-1">Contract Value</label>
            <input
              type="number"
              min={0}
              step="0.01"
              className={inputCls}
              value={form.value ?? ''}
              onChange={(e) => setForm(f => ({
                ...f, value: e.target.value ? parseFloat(e.target.value) : null
              }))}
              placeholder="e.g. 500000000"
            />
          </div>

          <div>
            <label className="block text-sm font-medium mb-1">Notes</label>
            <textarea
              rows={3}
              className={inputCls}
              value={form.notes ?? ''}
              onChange={(e) => setForm(f => ({ ...f, notes: e.target.value || null }))}
              placeholder="Any additional notes or terms..."
            />
          </div>

          <div className="flex justify-end gap-2 pt-1 border-t border-slate-100">
            <Button type="button" variant="outline" onClick={handleClose}>Cancel</Button>
            <Button type="submit" disabled={createMut.isPending}>
              {createMut.isPending ? 'Creating…' : 'Create Contract'}
            </Button>
          </div>
        </form>
      </DialogContent>
    </Dialog>
  );
}
