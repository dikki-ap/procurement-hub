import { useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { Shield, Plus } from 'lucide-react';
import { toast } from 'sonner';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
} from '@/components/ui/dialog';
import { approvalApi } from '../api/approvalApi';
import { useAuthStore } from '@/stores/authStore';
import { extractApiError } from '@/shared/lib/apiError';

const fmt = (n: number) =>
  new Intl.NumberFormat('id-ID', { style: 'decimal', minimumFractionDigits: 0 }).format(n);

const EMPTY_FORM = {
  referenceType: 'PR',
  name: '',
  minValue: '0',
  maxValue: '',
  requiredLevels: '1',
  isStrategicOverride: false,
  isSingleSourceOverride: false,
};

export default function ApprovalPoliciesPage() {
  const queryClient = useQueryClient();
  const { user }    = useAuthStore();
  const companyId   = user?.companyId ?? '';

  const [showModal, setShowModal] = useState(false);
  const [form, setForm] = useState(EMPTY_FORM);

  const { data: policies = [], isLoading } = useQuery({
    queryKey: ['approval-policies', companyId],
    queryFn:  () => approvalApi.getPolicies(companyId),
    enabled:  !!companyId,
  });

  const createMut = useMutation({
    mutationFn: () => approvalApi.createPolicy({
      companyId,
      referenceType:          form.referenceType,
      name:                   form.name,
      minValue:               parseFloat(form.minValue),
      maxValue:               form.maxValue ? parseFloat(form.maxValue) : undefined,
      requiredLevels:         parseInt(form.requiredLevels),
      isStrategicOverride:    form.isStrategicOverride,
      isSingleSourceOverride: form.isSingleSourceOverride,
    }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['approval-policies', companyId] });
      toast.success('Policy created.', { duration: 3000 });
      setShowModal(false);
      setForm(EMPTY_FORM);
    },
    onError: (error: unknown) => toast.error(extractApiError(error, 'Failed to create policy')),
  });

  if (isLoading) return <div className="p-6 text-muted-foreground">Loading...</div>;

  return (
    <div className="space-y-6">
      <div className="flex flex-wrap items-center justify-between gap-3">
        <div className="flex items-center gap-2">
          <Shield className="h-5 w-5 text-muted-foreground flex-shrink-0" />
          <div>
            <h1 className="text-xl sm:text-2xl font-semibold">Approval Policies</h1>
            <p className="text-sm text-muted-foreground hidden sm:block">Configure multi-level approval rules</p>
          </div>
        </div>
        <Button size="sm" onClick={() => setShowModal(true)}>
          <Plus className="h-4 w-4 mr-1" /> Add Policy
        </Button>
      </div>

      {policies.length === 0 ? (
        <p className="text-sm text-muted-foreground">No policies configured yet. Add one to enable multi-level approvals.</p>
      ) : (
        <div className="rounded-md border overflow-hidden">
          <table className="w-full text-sm">
            <thead className="bg-muted/50">
              <tr>
                {['Policy Name', 'Type', 'Min Value', 'Max Value', 'Levels', 'Overrides', 'Active'].map(h => (
                  <th key={h} className="px-3 py-2 text-left font-medium text-muted-foreground">{h}</th>
                ))}
              </tr>
            </thead>
            <tbody>
              {policies.map(p => (
                <tr key={p.id} className="border-t">
                  <td className="px-3 py-2 font-medium">{p.name}</td>
                  <td className="px-3 py-2 text-muted-foreground">{p.referenceType}</td>
                  <td className="px-3 py-2">Rp {fmt(p.minValue)}</td>
                  <td className="px-3 py-2">{p.maxValue ? `Rp ${fmt(p.maxValue)}` : '∞'}</td>
                  <td className="px-3 py-2 text-center font-medium">{p.requiredLevels}</td>
                  <td className="px-3 py-2 text-xs text-muted-foreground">
                    {[p.isStrategicOverride && 'Strategic', p.isSingleSourceOverride && 'SingleSource'].filter(Boolean).join(', ') || '—'}
                  </td>
                  <td className="px-3 py-2">
                    <span className={`inline-flex px-2 py-0.5 rounded-full text-xs font-medium ${p.isActive ? 'bg-emerald-50 text-emerald-700' : 'bg-gray-100 text-gray-500'}`}>
                      {p.isActive ? 'Active' : 'Inactive'}
                    </span>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}

      <Dialog open={showModal} onOpenChange={(v) => { if (!v && !createMut.isPending) { setShowModal(false); setForm(EMPTY_FORM); } }}>
        <DialogContent className="max-w-lg">
          <DialogHeader>
            <DialogTitle>Add Approval Policy</DialogTitle>
          </DialogHeader>
          <div className="space-y-4 mt-2">
            <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
              <div className="space-y-1">
                <label className="text-xs font-medium text-muted-foreground">Reference Type</label>
                <select
                  className="flex h-9 w-full rounded-md border border-input bg-transparent px-3 py-1 text-sm shadow-sm"
                  value={form.referenceType}
                  onChange={e => setForm(p => ({ ...p, referenceType: e.target.value }))}
                >
                  {['PR', 'PO', 'RFQ'].map(t => <option key={t}>{t}</option>)}
                </select>
              </div>
              <div className="space-y-1">
                <label className="text-xs font-medium text-muted-foreground">Policy Name</label>
                <Input value={form.name} onChange={e => setForm(p => ({ ...p, name: e.target.value }))} placeholder="e.g. Small Purchase" />
              </div>
              <div className="space-y-1">
                <label className="text-xs font-medium text-muted-foreground">Min Value (Rp)</label>
                <Input type="number" min={0} value={form.minValue} onChange={e => setForm(p => ({ ...p, minValue: e.target.value }))} />
              </div>
              <div className="space-y-1">
                <label className="text-xs font-medium text-muted-foreground">Max Value (blank = unlimited)</label>
                <Input type="number" min={0} value={form.maxValue} onChange={e => setForm(p => ({ ...p, maxValue: e.target.value }))} placeholder="Unlimited" />
              </div>
              <div className="space-y-1">
                <label className="text-xs font-medium text-muted-foreground">Required Levels (1–5)</label>
                <Input type="number" min={1} max={5} value={form.requiredLevels} onChange={e => setForm(p => ({ ...p, requiredLevels: e.target.value }))} />
              </div>
              <div className="space-y-1">
                <label className="text-xs font-medium text-muted-foreground">Overrides</label>
                <div className="flex flex-col gap-2 pt-1">
                  <label className="flex items-center gap-2 text-sm cursor-pointer">
                    <input type="checkbox" checked={form.isStrategicOverride}
                      onChange={e => setForm(p => ({ ...p, isStrategicOverride: e.target.checked }))} />
                    Add level for strategic items
                  </label>
                  <label className="flex items-center gap-2 text-sm cursor-pointer">
                    <input type="checkbox" checked={form.isSingleSourceOverride}
                      onChange={e => setForm(p => ({ ...p, isSingleSourceOverride: e.target.checked }))} />
                    Add level for single-source
                  </label>
                </div>
              </div>
            </div>
            <div className="flex justify-end gap-2 pt-2">
              <Button variant="outline" onClick={() => { setShowModal(false); setForm(EMPTY_FORM); }} disabled={createMut.isPending}>
                Cancel
              </Button>
              <Button onClick={() => createMut.mutate()} disabled={createMut.isPending || !form.name}>
                {createMut.isPending ? 'Saving…' : 'Save Policy'}
              </Button>
            </div>
          </div>
        </DialogContent>
      </Dialog>
    </div>
  );
}
