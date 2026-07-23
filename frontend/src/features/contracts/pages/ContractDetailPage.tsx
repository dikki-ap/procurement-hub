import { useRef, useState } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import {
  ArrowLeft, FileText, CheckCircle2, XCircle, Upload,
  Download, Pencil, AlertTriangle, Clock, Building2,
} from 'lucide-react';
import { toast } from 'sonner';
import { Button } from '@/components/ui/button';
import {
  Dialog, DialogContent, DialogHeader, DialogTitle,
} from '@/components/ui/dialog';
import { fmtDate, fmtDateTime } from '@/shared/lib/date';
import { contractApi, type ContractDto, type ContractStatus, type UpdateContractRequest } from '../api/contractApi';
import { extractApiError } from '@/shared/lib/apiError';
import { useAuthStore } from '@/stores/authStore';

const inputCls =
  'w-full rounded-md border border-input bg-transparent px-3 py-2 text-sm shadow-sm transition-colors placeholder:text-muted-foreground focus-visible:outline-none focus-visible:ring-1 focus-visible:ring-ring disabled:cursor-not-allowed disabled:opacity-50';

const STATUS_CFG: Record<ContractStatus, { label: string; cls: string }> = {
  Draft:      { label: 'Draft',      cls: 'bg-slate-100 text-slate-600' },
  Active:     { label: 'Active',     cls: 'bg-emerald-50 text-emerald-700' },
  Expired:    { label: 'Expired',    cls: 'bg-amber-50 text-amber-700' },
  Terminated: { label: 'Terminated', cls: 'bg-red-50 text-red-700' },
};

const StatusBadge = ({ status }: { status: ContractStatus }) => {
  const cfg = STATUS_CFG[status];
  return (
    <span className={`inline-flex items-center px-2.5 py-1 rounded-full text-xs font-medium ${cfg.cls}`}>
      {cfg.label}
    </span>
  );
};

function InfoRow({ label, value }: { label: string; value: React.ReactNode }) {
  return (
    <div className="flex flex-col gap-0.5">
      <span className="text-xs text-slate-500 uppercase tracking-wide">{label}</span>
      <span className="text-sm font-medium text-slate-800">{value ?? '—'}</span>
    </div>
  );
}

export default function ContractDetailPage() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const qc = useQueryClient();
  const fileRef = useRef<HTMLInputElement>(null);
  const isPurchasing = useAuthStore(s =>
    s.user?.roles?.some(r => ['purchasing', 'super_admin'].includes(r)) ?? false);

  const [editing, setEditing] = useState(false);
  const [editForm, setEditForm] = useState<UpdateContractRequest | null>(null);
  const [showTerminate, setShowTerminate] = useState(false);
  const [terminateReason, setTerminateReason] = useState('');

  const { data: contract, isLoading } = useQuery({
    queryKey: ['contract', id],
    queryFn: () => contractApi.getById(id!),
    enabled: !!id,
  });

  const invalidate = () => qc.invalidateQueries({ queryKey: ['contract', id] });

  const openEdit = (c: ContractDto) => {
    setEditForm({
      title: c.title,
      poId: c.poId,
      startDate: c.startDate ? c.startDate.substring(0, 10) : null,
      endDate: c.endDate ? c.endDate.substring(0, 10) : null,
      value: c.value,
      currencyId: c.currencyId,
      notes: c.notes,
    });
    setEditing(true);
  };

  const updateMut = useMutation({
    mutationFn: () => contractApi.update(id!, editForm!),
    onSuccess: () => { toast.success('Contract updated'); invalidate(); setEditing(false); },
    onError: (e: unknown) => toast.error(extractApiError(e, 'Update failed')),
  });

  const activateMut = useMutation({
    mutationFn: () => contractApi.activate(id!),
    onSuccess: () => { toast.success('Contract activated'); invalidate(); },
    onError: (e: unknown) => toast.error(extractApiError(e, 'Activation failed')),
  });

  const terminateMut = useMutation({
    mutationFn: () => contractApi.terminate(id!, terminateReason || undefined),
    onSuccess: () => {
      toast.success('Contract terminated');
      invalidate();
      setShowTerminate(false);
      setTerminateReason('');
    },
    onError: (e: unknown) => toast.error(extractApiError(e, 'Termination failed')),
  });

  const uploadMut = useMutation({
    mutationFn: (file: File) => contractApi.upload(id!, file),
    onSuccess: () => { toast.success('Contract document uploaded'); invalidate(); },
    onError: (e: unknown) => toast.error(extractApiError(e, 'Upload failed')),
  });

  const downloadMut = useMutation({
    mutationFn: () => contractApi.getDownloadUrl(id!),
    onSuccess: (url) => window.open(url, '_blank'),
    onError: (e: unknown) => toast.error(extractApiError(e, 'Download failed')),
  });

  const handleFileChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0];
    if (file) uploadMut.mutate(file);
    e.target.value = '';
  };

  if (isLoading || !contract) {
    return (
      <div className="animate-pulse space-y-4">
        <div className="h-8 bg-slate-100 rounded w-1/3" />
        <div className="h-64 bg-slate-100 rounded" />
      </div>
    );
  }

  const isDraft      = contract.status === 'Draft';
  const isActive     = contract.status === 'Active';
  const isTerminable = isActive;
  const isActivatable = isDraft;

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex flex-wrap items-center justify-between gap-3">
        <div className="flex items-center gap-3">
          <button
            onClick={() => navigate('/app/contracts')}
            className="flex items-center gap-1 text-sm text-slate-500 hover:text-slate-800 transition-colors"
          >
            <ArrowLeft className="h-4 w-4" /> Contracts
          </button>
          <span className="text-slate-300">/</span>
          <span className="text-sm font-mono font-semibold text-slate-700">{contract.contractNumber}</span>
          <StatusBadge status={contract.status} />
        </div>

        {isPurchasing && (
          <div className="flex items-center gap-2">
            {isDraft && (
              <Button variant="outline" size="sm" onClick={() => openEdit(contract)}>
                <Pencil className="h-3.5 w-3.5 mr-1" /> Edit
              </Button>
            )}
            <input
              ref={fileRef}
              type="file"
              accept=".pdf,.docx,image/jpeg,image/png"
              className="hidden"
              onChange={handleFileChange}
            />
            <Button
              variant="outline"
              size="sm"
              onClick={() => fileRef.current?.click()}
              disabled={uploadMut.isPending}
            >
              <Upload className="h-3.5 w-3.5 mr-1" />
              {uploadMut.isPending ? 'Uploading…' : contract.hasFile ? 'Replace File' : 'Upload File'}
            </Button>
            {isActivatable && (
              <Button
                size="sm"
                onClick={() => activateMut.mutate()}
                disabled={activateMut.isPending}
                className="bg-emerald-600 hover:bg-emerald-700"
              >
                <CheckCircle2 className="h-3.5 w-3.5 mr-1" />
                {activateMut.isPending ? 'Activating…' : 'Activate'}
              </Button>
            )}
            {isTerminable && (
              <Button
                size="sm"
                variant="destructive"
                onClick={() => setShowTerminate(true)}
              >
                <XCircle className="h-3.5 w-3.5 mr-1" /> Terminate
              </Button>
            )}
          </div>
        )}
      </div>

      {/* Info card */}
      <div className="bg-white rounded-xl border border-slate-100 p-6">
        <div className="flex items-center justify-between mb-5">
          <h2 className="text-lg font-semibold text-slate-900">{contract.title}</h2>
          {contract.hasFile && (
            <Button
              variant="outline"
              size="sm"
              onClick={() => downloadMut.mutate()}
              disabled={downloadMut.isPending}
            >
              <Download className="h-3.5 w-3.5 mr-1" />
              {downloadMut.isPending ? 'Getting link…' : 'Download'}
            </Button>
          )}
        </div>

        <div className="grid grid-cols-1 sm:grid-cols-3 gap-5">
          <InfoRow
            label="Vendor"
            value={
              <div className="flex items-center gap-1.5">
                <Building2 className="h-3.5 w-3.5 text-slate-400 flex-shrink-0" />
                {contract.vendorName}
              </div>
            }
          />
          <InfoRow label="PO Number" value={contract.poNumber} />
          <InfoRow label="Start Date" value={fmtDate(contract.startDate)} />
          <InfoRow label="End Date" value={
            <span className={
              contract.status === 'Active' && contract.endDate
              && (new Date(contract.endDate).getTime() - Date.now()) / 86400000 <= 30
                ? 'text-amber-600 font-semibold'
                : undefined
            }>
              {fmtDate(contract.endDate)}
            </span>
          } />
          <InfoRow
            label="Value"
            value={contract.value != null
              ? contract.value.toLocaleString(undefined, { maximumFractionDigits: 2 })
              : null}
          />
          <InfoRow
            label="Signed At"
            value={contract.signedAt ? fmtDate(contract.signedAt) : null}
          />
        </div>

        {contract.notes && (
          <div className="mt-5 pt-5 border-t border-slate-100">
            <p className="text-xs text-slate-500 uppercase tracking-wide mb-1">Notes</p>
            <p className="text-sm text-slate-700 whitespace-pre-wrap">{contract.notes}</p>
          </div>
        )}

        {!contract.hasFile && (
          <div className="mt-5 flex items-center gap-2 p-3 rounded-lg bg-amber-50 border border-amber-200">
            <AlertTriangle className="h-4 w-4 text-amber-500 flex-shrink-0" />
            <p className="text-sm text-amber-700">No contract document uploaded yet.</p>
          </div>
        )}
      </div>

      {/* Audit footer */}
      <div className="bg-white rounded-xl border border-slate-100 p-4">
        <div className="flex flex-wrap gap-6 text-xs text-slate-500">
          <div className="flex items-center gap-1">
            <Clock className="h-3.5 w-3.5" />
            Created {fmtDateTime(contract.createdAt)}
            {contract.createdByName && <> by <strong className="text-slate-700">{contract.createdByName}</strong></>}
          </div>
          {contract.updatedAt !== contract.createdAt && (
            <div className="flex items-center gap-1">
              <Clock className="h-3.5 w-3.5" />
              Updated {fmtDateTime(contract.updatedAt)}
              {contract.updatedByName && <> by <strong className="text-slate-700">{contract.updatedByName}</strong></>}
            </div>
          )}
        </div>
      </div>

      {/* Edit modal */}
      <Dialog open={editing} onOpenChange={(v) => { if (!v) setEditing(false); }}>
        <DialogContent className="max-w-lg">
          <DialogHeader>
            <DialogTitle>Edit Contract</DialogTitle>
          </DialogHeader>
          {editForm && (
            <form
              onSubmit={(e) => { e.preventDefault(); updateMut.mutate(); }}
              className="space-y-4 mt-2"
            >
              <div>
                <label className="block text-sm font-medium mb-1">
                  Title <span className="text-red-500">*</span>
                </label>
                <input
                  required
                  className={inputCls}
                  value={editForm.title}
                  onChange={(e) => setEditForm(f => f ? { ...f, title: e.target.value } : f)}
                />
              </div>
              <div className="grid grid-cols-2 gap-4">
                <div>
                  <label className="block text-sm font-medium mb-1">Start Date</label>
                  <input
                    type="date"
                    className={inputCls}
                    value={editForm.startDate ?? ''}
                    onChange={(e) => setEditForm(f => f ? { ...f, startDate: e.target.value || null } : f)}
                  />
                </div>
                <div>
                  <label className="block text-sm font-medium mb-1">End Date</label>
                  <input
                    type="date"
                    className={inputCls}
                    value={editForm.endDate ?? ''}
                    onChange={(e) => setEditForm(f => f ? { ...f, endDate: e.target.value || null } : f)}
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
                  value={editForm.value ?? ''}
                  onChange={(e) => setEditForm(f => f ? {
                    ...f, value: e.target.value ? parseFloat(e.target.value) : null
                  } : f)}
                />
              </div>
              <div>
                <label className="block text-sm font-medium mb-1">Notes</label>
                <textarea
                  rows={3}
                  className={inputCls}
                  value={editForm.notes ?? ''}
                  onChange={(e) => setEditForm(f => f ? { ...f, notes: e.target.value || null } : f)}
                />
              </div>
              <div className="flex justify-end gap-2 pt-1 border-t border-slate-100">
                <Button type="button" variant="outline" onClick={() => setEditing(false)}>Cancel</Button>
                <Button type="submit" disabled={updateMut.isPending}>
                  {updateMut.isPending ? 'Saving…' : 'Save'}
                </Button>
              </div>
            </form>
          )}
        </DialogContent>
      </Dialog>

      {/* Terminate modal */}
      <Dialog open={showTerminate} onOpenChange={(v) => { if (!v) setShowTerminate(false); }}>
        <DialogContent className="max-w-sm">
          <DialogHeader>
            <DialogTitle>Terminate Contract</DialogTitle>
          </DialogHeader>
          <div className="mt-2 space-y-3">
            <p className="text-sm text-slate-600">
              Are you sure you want to terminate <strong>{contract.contractNumber}</strong>?
              This action cannot be undone.
            </p>
            <div>
              <label className="block text-sm font-medium mb-1">Reason (optional)</label>
              <textarea
                rows={3}
                className={inputCls}
                value={terminateReason}
                onChange={(e) => setTerminateReason(e.target.value)}
                placeholder="e.g. Vendor failed to meet SLA requirements"
              />
            </div>
            <div className="flex justify-end gap-2 pt-1">
              <Button variant="outline" onClick={() => setShowTerminate(false)}>Cancel</Button>
              <Button
                variant="destructive"
                disabled={terminateMut.isPending}
                onClick={() => terminateMut.mutate()}
              >
                {terminateMut.isPending ? 'Terminating…' : 'Terminate'}
              </Button>
            </div>
          </div>
        </DialogContent>
      </Dialog>
    </div>
  );
}
