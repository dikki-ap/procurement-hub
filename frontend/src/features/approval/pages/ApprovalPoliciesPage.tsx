import { useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { Shield, Plus } from 'lucide-react';
import { toast } from 'sonner';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { approvalApi } from '../api/approvalApi';
import { useAuthStore } from '@/stores/authStore';

const fmt = (n: number) =>
  new Intl.NumberFormat('id-ID', { style: 'decimal', minimumFractionDigits: 0 }).format(n);

export default function ApprovalPoliciesPage() {
  const queryClient = useQueryClient();
  const { user }    = useAuthStore();
  const companyId   = user?.companyId ?? '';

  const [showForm,  setShowForm]  = useState(false);
  const [form, setForm] = useState({
    referenceType:         'PR',
    name:                  '',
    minValue:              '0',
    maxValue:              '',
    requiredLevels:        '1',
    isStrategicOverride:   false,
    isSingleSourceOverride: false,
  });

  const { data: policies = [], isLoading } = useQuery({
    queryKey: ['approval-policies', companyId],
    queryFn:  () => approvalApi.getPolicies(companyId).then(r => r.data),
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
      toast.success('Policy created.');
      setShowForm(false);
      setForm({ referenceType: 'PR', name: '', minValue: '0', maxValue: '',
        requiredLevels: '1', isStrategicOverride: false, isSingleSourceOverride: false });
    },
    onError: (e: any) => toast.error(e?.response?.data?.message ?? 'Failed to create policy'),
  });

  if (isLoading) return <div className="p-6 text-muted-foreground">Loading...</div>;

  return (
    <div className="space-y-6 max-w-4xl">
      <div className="flex items-center gap-3">
        <Shield className="h-5 w-5 text-muted-foreground" />
        <h1 className="text-2xl font-semibold">Approval Policies</h1>
        <Button size="sm" className="ml-auto" onClick={() => setShowForm(v => !v)}>
          <Plus className="h-4 w-4 mr-1" /> Add Policy
        </Button>
      </div>

      {showForm && (
        <div className="rounded-lg border p-4 space-y-4 bg-muted/20">
          <h2 className="text-base font-semibold">New Policy</h2>
          <div className="grid grid-cols-2 gap-4">
            <div className="space-y-1">
              <label className="text-xs font-medium text-muted-foreground">Reference Type</label>
              <select
                className="w-full rounded-md border px-3 py-2 text-sm h-9 bg-background"
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
              <label className="text-xs font-medium text-muted-foreground">Max Value (Rp, blank = unlimited)</label>
              <Input type="number" min={0} value={form.maxValue} onChange={e => setForm(p => ({ ...p, maxValue: e.target.value }))} placeholder="Unlimited" />
            </div>
            <div className="space-y-1">
              <label className="text-xs font-medium text-muted-foreground">Required Levels (1–5)</label>
              <Input type="number" min={1} max={5} value={form.requiredLevels} onChange={e => setForm(p => ({ ...p, requiredLevels: e.target.value }))} />
            </div>
            <div className="space-y-2 pt-1">
              <label className="flex items-center gap-2 text-sm cursor-pointer">
                <input type="checkbox" checked={form.isStrategicOverride}
                  onChange={e => setForm(p => ({ ...p, isStrategicOverride: e.target.checked }))} />
                Add level for strategic items
              </label>
              <label className="flex items-center gap-2 text-sm cursor-pointer">
                <input type="checkbox" checked={form.isSingleSourceOverride}
                  onChange={e => setForm(p => ({ ...p, isSingleSourceOverride: e.target.checked }))} />
                Add level for single-source procurement
              </label>
            </div>
          </div>
          <div className="flex gap-2">
            <Button onClick={() => createMut.mutate()} disabled={createMut.isPending || !form.name}>
              {createMut.isPending ? 'Saving…' : 'Save Policy'}
            </Button>
            <Button variant="outline" onClick={() => setShowForm(false)}>Cancel</Button>
          </div>
        </div>
      )}

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
    </div>
  );
}
