import { useState } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import {
  ArrowLeft, FileText, User, Mail, Phone,
  Building2, Store, Hash, FileCheck, Tag,
  Download, Eye, Plus, Pencil, Trash2, Package,
} from 'lucide-react';
import { toast } from 'sonner';
import { Button } from '@/components/ui/button';
import {
  Dialog, DialogContent, DialogHeader, DialogTitle,
} from '@/components/ui/dialog';
import { vendorApi, type VendorStatus, type DocumentStatus } from '../api/vendorApi';
import { TierBadge, ScoreDisplay } from '../components/VendorBadges';
import { fmtDate, fmtDateTime } from '@/shared/lib/date';
import { materialCategoryApi } from '@/features/master-data/material-category/api/materialCategoryApi';
import { SearchableSelect } from '@/shared/components/SearchableSelect';
import { extractApiError } from '@/shared/lib/apiError';

const COMPANY_ID = '00000000-0000-0000-0000-000000000001';

const inputCls =
  'w-full rounded-md border border-input bg-transparent px-3 py-2 text-sm shadow-sm transition-colors placeholder:text-muted-foreground focus-visible:outline-none focus-visible:ring-1 focus-visible:ring-ring disabled:cursor-not-allowed disabled:opacity-50';

// ── status badges ────────────────────────────────────────────────────────────

const StatusBadge = ({ status }: { status: VendorStatus }) => {
  const cfg: Record<VendorStatus, string> = {
    Pending:     'bg-amber-50 text-amber-700',
    Active:      'bg-emerald-50 text-emerald-700',
    Suspended:   'bg-orange-50 text-orange-700',
    Blacklisted: 'bg-red-50 text-red-700',
  };
  return (
    <span className={`inline-flex items-center px-2.5 py-1 rounded-full text-xs font-medium ${cfg[status]}`}>
      {status}
    </span>
  );
};

const DocStatusBadge = ({ status }: { status: DocumentStatus }) => {
  const cfg: Record<DocumentStatus, string> = {
    Active:  'bg-emerald-50 text-emerald-700',
    Expired: 'bg-red-50 text-red-700',
    Revoked: 'bg-slate-100 text-slate-500',
  };
  return (
    <span className={`inline-flex items-center px-2 py-0.5 rounded-full text-xs font-medium ${cfg[status]}`}>
      {status}
    </span>
  );
};

// ── tab definition ────────────────────────────────────────────────────────────

const tabs = ['Info', 'Contacts', 'Documents', 'Capabilities'] as const;
type Tab = typeof tabs[number];

// ── info field metadata ───────────────────────────────────────────────────────

type InfoField = { label: string; value: string; Icon: React.ElementType };

// ── capability modal state ────────────────────────────────────────────────────

type CapForm = { materialCategoryId: string; minOrderQty: string; leadTimeDays: string; notes: string };
const emptyCapForm = (): CapForm => ({ materialCategoryId: '', minOrderQty: '', leadTimeDays: '', notes: '' });

// ── main component ────────────────────────────────────────────────────────────

export default function VendorDetailPage() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const qc = useQueryClient();
  const [activeTab, setActiveTab] = useState<Tab>('Info');

  // document preview state
  const [previewUrl, setPreviewUrl]   = useState<string | null>(null);
  const [previewName, setPreviewName] = useState<string | null>(null);

  // capability modal state
  const [capModalOpen, setCapModalOpen]   = useState(false);
  const [editingCapId, setEditingCapId]   = useState<string | null>(null);
  const [capForm, setCapForm]             = useState<CapForm>(emptyCapForm());

  // ── queries ──────────────────────────────────────────────────────────────────

  const { data: vendor, isLoading } = useQuery({
    queryKey: ['vendor', id],
    queryFn:  () => vendorApi.getById(id!),
    enabled:  !!id,
  });

  const { data: categories = [] } = useQuery({
    queryKey: ['material-categories', COMPANY_ID],
    queryFn:  () => materialCategoryApi.getAll(COMPANY_ID),
    staleTime: 5 * 60 * 1000,
  });

  const categoryMap = new Map(categories.map(c => [c.id, c.name]));
  const categoryOptions = categories
    .filter(c => c.isActive)
    .map(c => ({ value: c.id, label: `${c.code} — ${c.name}` }));

  // ── capability mutations ─────────────────────────────────────────────────────

  const addCapMutation = useMutation({
    mutationFn: () => vendorApi.addCapability(id!, {
      materialCategoryId: capForm.materialCategoryId,
      minOrderQty:   capForm.minOrderQty   ? parseFloat(capForm.minOrderQty)   : null,
      leadTimeDays:  capForm.leadTimeDays  ? parseInt(capForm.leadTimeDays)    : null,
      notes:         capForm.notes || null,
    }),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['vendor', id] });
      toast.success('Capability added');
      setCapModalOpen(false);
      setCapForm(emptyCapForm());
    },
    onError: (e: unknown) => toast.error(extractApiError(e, 'Failed to add capability')),
  });

  const updateCapMutation = useMutation({
    mutationFn: () => vendorApi.updateCapability(id!, editingCapId!, {
      minOrderQty:   capForm.minOrderQty   ? parseFloat(capForm.minOrderQty)   : null,
      leadTimeDays:  capForm.leadTimeDays  ? parseInt(capForm.leadTimeDays)    : null,
      notes:         capForm.notes || null,
    }),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['vendor', id] });
      toast.success('Capability updated');
      setCapModalOpen(false);
      setEditingCapId(null);
      setCapForm(emptyCapForm());
    },
    onError: (e: unknown) => toast.error(extractApiError(e, 'Failed to update capability')),
  });

  const deleteCapMutation = useMutation({
    mutationFn: (capId: string) => vendorApi.deleteCapability(id!, capId),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['vendor', id] });
      toast.success('Capability removed');
    },
    onError: (e: unknown) => toast.error(extractApiError(e, 'Failed to remove capability')),
  });

  // ── document actions ─────────────────────────────────────────────────────────

  const handleDownload = async (docId: string, fileName: string | null) => {
    try {
      const { url } = await vendorApi.getDocumentUrl(id!, docId, false);
      window.open(url, '_blank', 'noopener,noreferrer');
    } catch {
      toast.error('Failed to download file');
    }
  };

  const handlePreview = async (docId: string, fileName: string | null) => {
    try {
      const { url } = await vendorApi.getDocumentUrl(id!, docId, true);
      setPreviewName(fileName);
      setPreviewUrl(url);
    } catch {
      toast.error('Failed to load preview');
    }
  };

  // ── helpers ──────────────────────────────────────────────────────────────────

  const openAddCap = () => {
    setEditingCapId(null);
    setCapForm(emptyCapForm());
    setCapModalOpen(true);
  };

  const openEditCap = (cap: { id: string; materialCategoryId: string; minOrderQty: number | null; leadTimeDays: number | null; notes: string | null }) => {
    setEditingCapId(cap.id);
    setCapForm({
      materialCategoryId: cap.materialCategoryId,
      minOrderQty:  cap.minOrderQty  != null ? String(cap.minOrderQty)  : '',
      leadTimeDays: cap.leadTimeDays != null ? String(cap.leadTimeDays) : '',
      notes:        cap.notes ?? '',
    });
    setCapModalOpen(true);
  };

  const handleCapSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    if (editingCapId) updateCapMutation.mutate();
    else              addCapMutation.mutate();
  };

  const isPdf = (name: string | null) => name?.toLowerCase().endsWith('.pdf') ?? false;

  // ── render ───────────────────────────────────────────────────────────────────

  if (isLoading) {
    return (
      <div className="animate-pulse space-y-4">
        <div className="h-8 bg-slate-100 rounded w-1/3" />
        <div className="h-32 bg-slate-100 rounded" />
      </div>
    );
  }

  if (!vendor) return null;

  const infoFields: InfoField[] = [
    { label: 'Legal Name',   value: vendor.legalName,         Icon: Building2  },
    { label: 'Trade Name',   value: vendor.tradeName ?? '—',  Icon: Store      },
    { label: 'NPWP',         value: vendor.npwp    ?? '—',    Icon: Hash       },
    { label: 'SIUP',         value: vendor.siup    ?? '—',    Icon: FileCheck  },
    { label: 'NIB',          value: vendor.nib     ?? '—',    Icon: Hash       },
    { label: 'Vendor Type',  value: vendor.vendorType,        Icon: Tag        },
  ];

  return (
    <div>
      {/* ── page header ── */}
      <div className="flex items-center gap-3 mb-6">
        <Button variant="ghost" size="icon" onClick={() => navigate(-1)}>
          <ArrowLeft className="h-4 w-4" />
        </Button>
        <div>
          <div className="flex items-center gap-3">
            <h1 className="text-xl font-semibold text-slate-900">{vendor.legalName}</h1>
            <StatusBadge status={vendor.status} />
          </div>
          <p className="text-sm text-slate-500 mt-0.5">{vendor.vendorCode} · {vendor.vendorType}</p>
        </div>
      </div>

      {/* ── score card row ── */}
      <div className="grid grid-cols-3 gap-4 mb-6">
        <div className="bg-white rounded-xl border border-slate-100 p-4">
          <p className="text-xs text-slate-500 mb-1.5">Score</p>
          <ScoreDisplay score={vendor.score} />
        </div>
        <div className="bg-white rounded-xl border border-slate-100 p-4">
          <p className="text-xs text-slate-500 mb-1.5">Tier</p>
          <TierBadge tier={vendor.tier} />
        </div>
        <div className="bg-white rounded-xl border border-slate-100 p-4">
          <p className="text-xs text-slate-500 mb-1.5">Approved</p>
          <p className="text-sm font-semibold text-slate-900">{fmtDateTime(vendor.approvedAt)}</p>
        </div>
      </div>

      {/* ── tabs ── */}
      <div className="border-b border-slate-200 mb-6">
        <div className="flex gap-1">
          {tabs.map((tab) => (
            <button
              key={tab}
              onClick={() => setActiveTab(tab)}
              className={`px-4 py-2.5 text-sm font-medium border-b-2 transition-colors ${
                activeTab === tab
                  ? 'border-blue-500 text-blue-600'
                  : 'border-transparent text-slate-500 hover:text-slate-700'
              }`}
            >
              {tab}
            </button>
          ))}
        </div>
      </div>

      {/* ── Info tab ── */}
      {activeTab === 'Info' && (
        <div className="grid grid-cols-2 gap-6">
          {infoFields.map(({ label, value, Icon }) => (
            <div key={label}>
              <p className="flex items-center text-xs text-slate-500 mb-0.5">
                <Icon className="h-3.5 w-3.5 mr-1 text-slate-400" />
                {label}
              </p>
              <p className="text-sm font-medium text-slate-800">{value}</p>
            </div>
          ))}
          {vendor.isBlacklisted && (
            <div className="col-span-2 bg-red-50 rounded-lg p-4">
              <p className="text-xs font-semibold text-red-600 mb-1">Blacklist Reason</p>
              <p className="text-sm text-red-700">{vendor.blacklistReason}</p>
            </div>
          )}
        </div>
      )}

      {/* ── Contacts tab ── */}
      {activeTab === 'Contacts' && (
        <div className="space-y-3">
          {vendor.contacts.length === 0 ? (
            <p className="text-sm text-slate-500">No contacts on record.</p>
          ) : (
            vendor.contacts.map((c) => (
              <div key={c.id} className="flex items-start gap-3 bg-white rounded-xl border border-slate-100 p-4">
                <div className="w-8 h-8 rounded-full bg-blue-100 flex items-center justify-center text-blue-600 flex-shrink-0">
                  <User className="h-4 w-4" />
                </div>
                <div className="flex-1">
                  <p className="text-sm font-medium text-slate-900">
                    {c.name}
                    {c.isPrimary && (
                      <span className="ml-2 text-xs bg-blue-50 text-blue-600 px-1.5 py-0.5 rounded">Primary</span>
                    )}
                  </p>
                  {c.position && <p className="text-xs text-slate-500">{c.position}</p>}
                  <div className="flex flex-wrap gap-4 mt-1.5">
                    {c.email && (
                      <span className="flex items-center text-xs text-slate-500">
                        <Mail className="h-3 w-3 mr-1 text-slate-400" />
                        {c.email}
                      </span>
                    )}
                    {c.phone && (
                      <span className="flex items-center text-xs text-slate-500">
                        <Phone className="h-3 w-3 mr-1 text-slate-400" />
                        {c.phone}
                      </span>
                    )}
                  </div>
                </div>
              </div>
            ))
          )}
        </div>
      )}

      {/* ── Documents tab ── */}
      {activeTab === 'Documents' && (
        <div className="space-y-3">
          {vendor.documents.length === 0 ? (
            <p className="text-sm text-slate-500">No documents on record.</p>
          ) : (
            vendor.documents.map((d) => (
              <div key={d.id} className="flex items-center gap-4 bg-white rounded-xl border border-slate-100 p-4">
                <FileText className="h-5 w-5 text-slate-400 flex-shrink-0" />
                <div className="flex-1 min-w-0">
                  <p className="text-sm font-medium text-slate-900">{d.documentType}</p>
                  {d.documentNumber && <p className="text-xs text-slate-500">#{d.documentNumber}</p>}
                  <p className="text-xs text-slate-400 mt-0.5">{d.fileName ?? 'Unknown file'}</p>
                </div>
                <div className="text-right">
                  <DocStatusBadge status={d.status} />
                  {d.expiredAt && (
                    <p className="text-xs text-slate-400 mt-1">Exp: {fmtDate(d.expiredAt)}</p>
                  )}
                </div>
                <div className="flex items-center gap-1.5 ml-2">
                  <Button
                    variant="ghost" size="sm"
                    className="text-slate-500 hover:text-blue-600"
                    onClick={() => handlePreview(d.id, d.fileName)}
                  >
                    <Eye className="h-3.5 w-3.5 mr-1" /> Preview
                  </Button>
                  <Button
                    variant="ghost" size="sm"
                    className="text-slate-500 hover:text-emerald-600"
                    onClick={() => handleDownload(d.id, d.fileName)}
                  >
                    <Download className="h-3.5 w-3.5 mr-1" /> Download
                  </Button>
                </div>
              </div>
            ))
          )}
        </div>
      )}

      {/* ── Capabilities tab ── */}
      {activeTab === 'Capabilities' && (
        <div className="space-y-3">
          <div className="flex items-center justify-between">
            <p className="text-xs text-slate-500">
              Approved supply categories for this vendor
            </p>
            <Button size="sm" onClick={openAddCap}>
              <Plus className="h-3.5 w-3.5 mr-1" /> Add Capability
            </Button>
          </div>

          {vendor.capabilities.length === 0 ? (
            <div className="flex flex-col items-center justify-center py-12 text-slate-400">
              <Package className="h-10 w-10 mb-3" />
              <p className="text-sm">No capabilities registered yet.</p>
            </div>
          ) : (
            vendor.capabilities.map((cap) => (
              <div key={cap.id} className="flex items-start gap-4 bg-white rounded-xl border border-slate-100 p-4">
                <div className="w-8 h-8 rounded-full bg-indigo-50 flex items-center justify-center text-indigo-500 flex-shrink-0">
                  <Package className="h-4 w-4" />
                </div>
                <div className="flex-1">
                  <p className="text-sm font-medium text-slate-900">
                    {categoryMap.get(cap.materialCategoryId) ?? cap.materialCategoryId}
                  </p>
                  <div className="flex flex-wrap gap-4 mt-1 text-xs text-slate-500">
                    {cap.minOrderQty  != null && <span>Min Order: <strong>{cap.minOrderQty}</strong></span>}
                    {cap.leadTimeDays != null && <span>Lead Time: <strong>{cap.leadTimeDays}d</strong></span>}
                    {cap.notes && <span className="italic">{cap.notes}</span>}
                  </div>
                </div>
                <div className="flex items-center gap-1">
                  <Button
                    variant="ghost" size="icon"
                    className="h-7 w-7 text-slate-400 hover:text-blue-600"
                    onClick={() => openEditCap(cap)}
                  >
                    <Pencil className="h-3.5 w-3.5" />
                  </Button>
                  <Button
                    variant="ghost" size="icon"
                    className="h-7 w-7 text-slate-400 hover:text-red-500"
                    onClick={() => deleteCapMutation.mutate(cap.id)}
                    disabled={deleteCapMutation.isPending}
                  >
                    <Trash2 className="h-3.5 w-3.5" />
                  </Button>
                </div>
              </div>
            ))
          )}
        </div>
      )}

      {/* ── Document Preview Modal ── */}
      <Dialog open={!!previewUrl} onOpenChange={(v) => { if (!v) { setPreviewUrl(null); setPreviewName(null); } }}>
        <DialogContent className="max-w-4xl max-h-[90vh] flex flex-col">
          <DialogHeader>
            <DialogTitle>{previewName ?? 'Document Preview'}</DialogTitle>
          </DialogHeader>
          <div className="flex-1 overflow-hidden rounded-md border border-slate-200 min-h-[60vh]">
            {previewUrl && isPdf(previewName) ? (
              <iframe src={previewUrl} className="w-full h-full" style={{ minHeight: '60vh' }} title="document preview" />
            ) : previewUrl ? (
              <div className="flex items-center justify-center h-full p-4">
                <img src={previewUrl} alt={previewName ?? 'document'} className="max-w-full max-h-[65vh] object-contain rounded" />
              </div>
            ) : null}
          </div>
        </DialogContent>
      </Dialog>

      {/* ── Capability Add/Edit Modal ── */}
      <Dialog open={capModalOpen} onOpenChange={(v) => { if (!v) { setCapModalOpen(false); setEditingCapId(null); setCapForm(emptyCapForm()); } }}>
        <DialogContent className="max-w-md">
          <DialogHeader>
            <DialogTitle>{editingCapId ? 'Edit Capability' : 'Add Capability'}</DialogTitle>
          </DialogHeader>
          <form onSubmit={handleCapSubmit} className="space-y-4 mt-2">
            <div>
              <label className="block text-sm font-medium mb-1">
                Material Category <span className="text-red-500">*</span>
              </label>
              <SearchableSelect
                options={categoryOptions}
                value={capForm.materialCategoryId}
                onChange={(v) => setCapForm(f => ({ ...f, materialCategoryId: v }))}
                placeholder="Search category..."
                disabled={!!editingCapId}
              />
            </div>
            <div className="grid grid-cols-2 gap-3">
              <div>
                <label className="block text-sm font-medium mb-1">Min Order Qty</label>
                <input
                  type="number" min="0" step="0.01"
                  className={inputCls}
                  value={capForm.minOrderQty}
                  onChange={(e) => setCapForm(f => ({ ...f, minOrderQty: e.target.value }))}
                  placeholder="e.g. 10"
                />
              </div>
              <div>
                <label className="block text-sm font-medium mb-1">Lead Time (days)</label>
                <input
                  type="number" min="0" step="1"
                  className={inputCls}
                  value={capForm.leadTimeDays}
                  onChange={(e) => setCapForm(f => ({ ...f, leadTimeDays: e.target.value }))}
                  placeholder="e.g. 7"
                />
              </div>
            </div>
            <div>
              <label className="block text-sm font-medium mb-1">Notes</label>
              <textarea
                rows={2}
                className={inputCls}
                value={capForm.notes}
                onChange={(e) => setCapForm(f => ({ ...f, notes: e.target.value }))}
                placeholder="Optional notes..."
              />
            </div>
            <div className="flex justify-end gap-2 pt-1">
              <Button
                type="button" variant="outline"
                onClick={() => { setCapModalOpen(false); setEditingCapId(null); setCapForm(emptyCapForm()); }}
              >
                Cancel
              </Button>
              <Button
                type="submit"
                disabled={addCapMutation.isPending || updateCapMutation.isPending || (!editingCapId && !capForm.materialCategoryId)}
              >
                {editingCapId ? 'Update' : 'Add'}
              </Button>
            </div>
          </form>
        </DialogContent>
      </Dialog>
    </div>
  );
}
