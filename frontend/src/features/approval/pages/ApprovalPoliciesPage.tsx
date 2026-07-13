import { useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { Shield, Plus, Pencil, Trash2 } from 'lucide-react';
import { toast } from 'sonner';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Dialog, DialogContent, DialogHeader, DialogTitle } from '@/components/ui/dialog';
import { DataTable, type DataTableColumn } from '@/shared/components/DataTable';
import { ConfirmDeleteModal } from '@/shared/components/ConfirmDeleteModal';
import { approvalApi, type ApprovalPolicyDto } from '../api/approvalApi';
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
  isActive: true,
};

type FormState = typeof EMPTY_FORM;
type ModalState = { mode: 'add' } | { mode: 'edit'; policy: ApprovalPolicyDto } | null;

const StatusBadge = ({ active }: { active: boolean }) => (
  <span className={`inline-flex items-center px-2 py-0.5 rounded-full text-xs font-medium ${active ? 'bg-emerald-50 text-emerald-700' : 'bg-slate-100 text-slate-500'}`}>
    {active ? 'Active' : 'Inactive'}
  </span>
);

export default function ApprovalPoliciesPage() {
  const queryClient = useQueryClient();
  const { user }    = useAuthStore();
  const companyId   = user?.companyId ?? '';

  const [modal, setModal]             = useState<ModalState>(null);
  const [deleteTarget, setDeleteTarget] = useState<ApprovalPolicyDto | null>(null);
  const [form, setForm]               = useState<FormState>(EMPTY_FORM);

  const { data: policies = [], isLoading } = useQuery({
    queryKey: ['approval-policies', companyId],
    queryFn:  () => approvalApi.getPolicies(companyId),
    enabled:  !!companyId,
  });

  const openAdd = () => { setForm(EMPTY_FORM); setModal({ mode: 'add' }); };
  const openEdit = (p: ApprovalPolicyDto) => {
    setForm({
      referenceType:          p.referenceType,
      name:                   p.name,
      minValue:               String(p.minValue),
      maxValue:               p.maxValue != null ? String(p.maxValue) : '',
      requiredLevels:         String(p.requiredLevels),
      isStrategicOverride:    p.isStrategicOverride,
      isSingleSourceOverride: p.isSingleSourceOverride,
      isActive:               p.isActive,
    });
    setModal({ mode: 'edit', policy: p });
  };
  const closeModal = () => setModal(null);

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
      closeModal();
    },
    onError: (e: unknown) => toast.error(extractApiError(e, 'Failed to create policy')),
  });

  const updateMut = useMutation({
    mutationFn: () => {
      const p = (modal as { mode: 'edit'; policy: ApprovalPolicyDto }).policy;
      return approvalApi.updatePolicy(p.id, {
        referenceType:          form.referenceType,
        name:                   form.name,
        minValue:               parseFloat(form.minValue),
        maxValue:               form.maxValue ? parseFloat(form.maxValue) : undefined,
        requiredLevels:         parseInt(form.requiredLevels),
        isStrategicOverride:    form.isStrategicOverride,
        isSingleSourceOverride: form.isSingleSourceOverride,
        isActive:               form.isActive,
      });
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['approval-policies', companyId] });
      toast.success('Policy updated.');
      closeModal();
    },
    onError: (e: unknown) => toast.error(extractApiError(e, 'Failed to update policy')),
  });

  const deleteMut = useMutation({
    mutationFn: (id: string) => approvalApi.deletePolicy(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['approval-policies', companyId] });
      toast.success('Policy deleted.');
      setDeleteTarget(null);
    },
    onError: (e: unknown) => toast.error(extractApiError(e, 'Failed to delete policy')),
  });

  const isEdit    = modal?.mode === 'edit';
  const isPending = createMut.isPending || updateMut.isPending;
  const onSubmit  = () => isEdit ? updateMut.mutate() : createMut.mutate();

  const columns: DataTableColumn<ApprovalPolicyDto>[] = [
    { key: 'name',          header: 'Policy Name', sortable: true,
      render: (r) => <span className="font-medium">{r.name}</span> },
    { key: 'referenceType', header: 'Type', sortable: true,
      render: (r) => <span className="text-xs font-mono bg-slate-100 px-1.5 py-0.5 rounded">{r.referenceType}</span> },
    { key: 'minValue',      header: 'Min Value',
      render: (r) => <span className="text-xs text-slate-600">Rp {fmt(r.minValue)}</span> },
    { key: 'maxValue',      header: 'Max Value',
      render: (r) => <span className="text-xs text-slate-600">{r.maxValue != null ? `Rp ${fmt(r.maxValue)}` : '∞'}</span> },
    { key: 'requiredLevels', header: 'Levels',
      render: (r) => <span className="font-medium text-center block">{r.requiredLevels}</span> },
    { key: 'isStrategicOverride', header: 'Overrides',
      render: (r) => (
        <span className="text-xs text-slate-500">
          {[r.isStrategicOverride && 'Strategic', r.isSingleSourceOverride && 'Single Source']
            .filter(Boolean).join(', ') || '—'}
        </span>
      ) },
    { key: 'isActive', header: 'Status', render: (r) => <StatusBadge active={r.isActive} /> },
  ];

  return (
    <div>
      <div className="flex flex-wrap items-center justify-between gap-3 mb-6">
        <div className="flex items-center gap-2">
          <Shield className="h-5 w-5 text-muted-foreground flex-shrink-0" />
          <div>
            <h1 className="text-xl sm:text-2xl font-semibold">Approval Policies</h1>
            <p className="text-sm text-muted-foreground hidden sm:block">Configure multi-level approval rules</p>
          </div>
        </div>
        <Button size="sm" onClick={openAdd} className="gap-1">
          <Plus className="h-4 w-4" /> Add Policy
        </Button>
      </div>

      <DataTable
        data={policies as unknown as Record<string, unknown>[]}
        columns={columns as DataTableColumn<Record<string, unknown>>[]}
        isLoading={isLoading}
        searchPlaceholder="Search policies..."
        emptyMessage="No approval policies configured yet."
        rowActions={(row) => {
          const p = row as unknown as ApprovalPolicyDto;
          return (
            <>
              <Button variant="ghost" size="icon" className="h-8 w-8" onClick={() => openEdit(p)}>
                <Pencil className="h-3.5 w-3.5" />
              </Button>
              <Button variant="ghost" size="icon" className="h-8 w-8 text-red-500 hover:text-red-600"
                onClick={() => setDeleteTarget(p)}>
                <Trash2 className="h-3.5 w-3.5" />
              </Button>
            </>
          );
        }}
      />

      {/* Add / Edit modal */}
      <Dialog open={modal !== null} onOpenChange={(v) => { if (!v && !isPending) closeModal(); }}>
        <DialogContent className="max-w-lg">
          <DialogHeader>
            <DialogTitle>{isEdit ? 'Edit Approval Policy' : 'Add Approval Policy'}</DialogTitle>
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
            {isEdit && (
              <div className="flex items-center gap-2">
                <input type="checkbox" id="isActive" checked={form.isActive}
                  onChange={e => setForm(p => ({ ...p, isActive: e.target.checked }))} />
                <label htmlFor="isActive" className="text-sm cursor-pointer">Active</label>
              </div>
            )}
            <div className="flex justify-end gap-2 pt-2">
              <Button variant="outline" onClick={closeModal} disabled={isPending}>Cancel</Button>
              <Button onClick={onSubmit} disabled={isPending || !form.name}>
                {isPending ? 'Saving…' : isEdit ? 'Update' : 'Save Policy'}
              </Button>
            </div>
          </div>
        </DialogContent>
      </Dialog>

      <ConfirmDeleteModal
        open={deleteTarget !== null}
        title="Delete Approval Policy"
        description={`Delete "${deleteTarget?.name}"? This cannot be undone.`}
        isPending={deleteMut.isPending}
        onConfirm={() => deleteTarget && deleteMut.mutate(deleteTarget.id)}
        onCancel={() => setDeleteTarget(null)}
      />
    </div>
  );
}
