import { useState } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import {
  ArrowLeft, FileText, User, Mail, Phone,
  Building2, Store, Hash, FileCheck, Tag,
  Download, Eye, Plus, Pencil, Trash2, Package,
  MapPin, CreditCard, AlertTriangle, CheckCircle2, Landmark, TrendingUp,
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
import { useAuthStore } from '@/stores/authStore';

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

const tabs = ['Info', 'Contacts', 'Documents', 'Capabilities', 'Bank Accounts', 'Score History'] as const;
type Tab = typeof tabs[number];

// ── capability modal state ────────────────────────────────────────────────────

type CapForm = {
  materialCategoryId: string;
  minOrderQty: string;
  maxOrderQty: string;
  uom: string;
  leadTimeDays: string;
  effectiveDate: string;
  expiryDate: string;
  notes: string;
};
const emptyCapForm = (): CapForm => ({
  materialCategoryId: '', minOrderQty: '', maxOrderQty: '', uom: '',
  leadTimeDays: '', effectiveDate: '', expiryDate: '', notes: '',
});

const UOM_OPTIONS = ['pcs', 'kg', 'ton', 'liter', 'm', 'm²', 'm³', 'box', 'set', 'unit'];

// ── bank account modal state ──────────────────────────────────────────────────

const CURRENCY_OPTIONS = ['IDR', 'USD', 'EUR', 'SGD'];

type BankForm = {
  bankName: string;
  accountNumber: string;
  accountName: string;
  branchName: string;
  currency: string;
  isDefault: boolean;
  notes: string;
};
const emptyBankForm = (): BankForm => ({
  bankName: '', accountNumber: '', accountName: '', branchName: '',
  currency: 'IDR', isDefault: false, notes: '',
});

// ── main component ────────────────────────────────────────────────────────────

export default function VendorDetailPage() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const qc = useQueryClient();
  const companyId = useAuthStore(s => s.user?.companyId ?? '');
  const isMasterData = useAuthStore(s =>
    s.user?.roles?.some(r => ['master_data', 'super_admin'].includes(r)) ?? false);
  const [activeTab, setActiveTab] = useState<Tab>('Info');

  // tax edit modal state
  const [taxModalOpen, setTaxModalOpen] = useState(false);
  const [taxIsPkp, setTaxIsPkp]       = useState(false);
  const [taxPphRate, setTaxPphRate]    = useState('');

  // document preview state
  const [previewUrl, setPreviewUrl]   = useState<string | null>(null);
  const [previewName, setPreviewName] = useState<string | null>(null);

  // capability modal state
  const [capModalOpen, setCapModalOpen] = useState(false);
  const [editingCapId, setEditingCapId] = useState<string | null>(null);
  const [capForm, setCapForm]           = useState<CapForm>(emptyCapForm());

  // bank account modal state
  const [bankModalOpen, setBankModalOpen] = useState(false);
  const [editingBankId, setEditingBankId] = useState<string | null>(null);
  const [bankForm, setBankForm]           = useState<BankForm>(emptyBankForm());

  // ── queries ──────────────────────────────────────────────────────────────────

  const { data: vendor, isLoading } = useQuery({
    queryKey: ['vendor', id],
    queryFn:  () => vendorApi.getById(id!),
    enabled:  !!id,
  });

  const { data: categories = [] } = useQuery({
    queryKey: ['material-categories', companyId],
    queryFn:  () => materialCategoryApi.getAll(companyId),
    enabled:  !!companyId,
    staleTime: 5 * 60 * 1000,
  });

  const categoryMap = new Map(categories.map(c => [c.id, c.name]));
  const categoryOptions = categories
    .filter(c => c.isActive)
    .map(c => ({ value: c.id, label: `${c.code} — ${c.name}` }));

  const { data: scoreHistory = [] } = useQuery({
    queryKey: ['vendor', id, 'scores'],
    queryFn:  () => vendorApi.getScoreHistory(id!),
    enabled:  !!id && activeTab === 'Score History',
  });

  // ── capability mutations ─────────────────────────────────────────────────────

  const addCapMutation = useMutation({
    mutationFn: () => vendorApi.addCapability(id!, {
      materialCategoryId: capForm.materialCategoryId,
      minOrderQty:   capForm.minOrderQty   ? parseFloat(capForm.minOrderQty)   : null,
      maxOrderQty:   capForm.maxOrderQty   ? parseFloat(capForm.maxOrderQty)   : null,
      uom:           capForm.uom           || null,
      leadTimeDays:  capForm.leadTimeDays  ? parseInt(capForm.leadTimeDays)    : null,
      effectiveDate: capForm.effectiveDate || null,
      expiryDate:    capForm.expiryDate    || null,
      notes:         capForm.notes         || null,
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
      maxOrderQty:   capForm.maxOrderQty   ? parseFloat(capForm.maxOrderQty)   : null,
      uom:           capForm.uom           || null,
      leadTimeDays:  capForm.leadTimeDays  ? parseInt(capForm.leadTimeDays)    : null,
      effectiveDate: capForm.effectiveDate || null,
      expiryDate:    capForm.expiryDate    || null,
      notes:         capForm.notes         || null,
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

  // ── bank account mutations ───────────────────────────────────────────────────

  const addBankMutation = useMutation({
    mutationFn: () => vendorApi.addBankAccount(id!, {
      bankName:      bankForm.bankName,
      accountNumber: bankForm.accountNumber,
      accountName:   bankForm.accountName,
      branchName:    bankForm.branchName || null,
      currency:      bankForm.currency,
      isDefault:     bankForm.isDefault,
      notes:         bankForm.notes || null,
    }),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['vendor', id] });
      toast.success('Bank account added');
      setBankModalOpen(false);
      setBankForm(emptyBankForm());
    },
    onError: (e: unknown) => toast.error(extractApiError(e, 'Failed to add bank account')),
  });

  const updateBankMutation = useMutation({
    mutationFn: () => vendorApi.updateBankAccount(id!, editingBankId!, {
      bankName:      bankForm.bankName,
      accountNumber: bankForm.accountNumber,
      accountName:   bankForm.accountName,
      branchName:    bankForm.branchName || null,
      currency:      bankForm.currency,
      isDefault:     bankForm.isDefault,
      notes:         bankForm.notes || null,
    }),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['vendor', id] });
      toast.success('Bank account updated');
      setBankModalOpen(false);
      setEditingBankId(null);
      setBankForm(emptyBankForm());
    },
    onError: (e: unknown) => toast.error(extractApiError(e, 'Failed to update bank account')),
  });

  const deleteBankMutation = useMutation({
    mutationFn: (bankId: string) => vendorApi.deleteBankAccount(id!, bankId),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['vendor', id] });
      toast.success('Bank account removed');
    },
    onError: (e: unknown) => toast.error(extractApiError(e, 'Failed to remove bank account')),
  });

  const updateTaxMutation = useMutation({
    mutationFn: () => vendorApi.updateTaxInfo(id!, taxIsPkp, taxPphRate ? parseFloat(taxPphRate) : null, vendor!),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['vendor', id] });
      toast.success('Tax settings updated');
      setTaxModalOpen(false);
    },
    onError: (e: unknown) => toast.error(extractApiError(e, 'Failed to update tax settings')),
  });

  const openTaxModal = () => {
    setTaxIsPkp(vendor?.isPkp ?? false);
    setTaxPphRate(vendor?.pphRate != null ? String(vendor.pphRate) : '');
    setTaxModalOpen(true);
  };

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

  const openAddCap = () => { setEditingCapId(null); setCapForm(emptyCapForm()); setCapModalOpen(true); };

  const openEditCap = (cap: typeof vendor extends undefined ? never : NonNullable<typeof vendor>['capabilities'][number]) => {
    setEditingCapId(cap.id);
    setCapForm({
      materialCategoryId: cap.materialCategoryId,
      minOrderQty:   cap.minOrderQty  != null ? String(cap.minOrderQty)  : '',
      maxOrderQty:   cap.maxOrderQty  != null ? String(cap.maxOrderQty)  : '',
      uom:           cap.uom ?? '',
      leadTimeDays:  cap.leadTimeDays != null ? String(cap.leadTimeDays) : '',
      effectiveDate: cap.effectiveDate ?? '',
      expiryDate:    cap.expiryDate    ?? '',
      notes:         cap.notes ?? '',
    });
    setCapModalOpen(true);
  };

  const openAddBank = () => { setEditingBankId(null); setBankForm(emptyBankForm()); setBankModalOpen(true); };

  const openEditBank = (b: NonNullable<typeof vendor>['bankAccounts'][number]) => {
    setEditingBankId(b.id);
    setBankForm({
      bankName:      b.bankName,
      accountNumber: b.accountNumber,
      accountName:   b.accountName,
      branchName:    b.branchName ?? '',
      currency:      b.currency,
      isDefault:     b.isDefault,
      notes:         b.notes ?? '',
    });
    setBankModalOpen(true);
  };

  const handleCapSubmit  = (e: React.FormEvent) => { e.preventDefault(); editingCapId  ? updateCapMutation.mutate()  : addCapMutation.mutate(); };
  const handleBankSubmit = (e: React.FormEvent) => { e.preventDefault(); editingBankId ? updateBankMutation.mutate() : addBankMutation.mutate(); };

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

  const addressLine = [vendor.address, vendor.city, vendor.province, vendor.postalCode, vendor.country]
    .filter(Boolean).join(', ');

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
      <div className="grid grid-cols-1 sm:grid-cols-3 gap-4 mb-6">
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
      <div className="border-b border-slate-200 mb-6 overflow-x-auto">
        <div className="flex gap-1">
          {tabs.map((tab) => (
            <button
              key={tab}
              onClick={() => setActiveTab(tab)}
              className={`whitespace-nowrap flex-shrink-0 px-4 py-2.5 text-sm font-medium border-b-2 transition-colors ${
                activeTab === tab
                  ? 'border-blue-500 text-blue-600'
                  : 'border-transparent text-slate-500 hover:text-slate-700'
              }`}
            >
              {tab}
              {tab === 'Bank Accounts' && vendor.bankAccounts.length > 0 && (
                <span className="ml-1.5 text-xs bg-slate-100 text-slate-500 px-1.5 py-0.5 rounded-full">
                  {vendor.bankAccounts.length}
                </span>
              )}
            </button>
          ))}
        </div>
      </div>

      {/* ── Info tab ── */}
      {activeTab === 'Info' && (
        <div className="space-y-6">
          <div className="grid grid-cols-1 sm:grid-cols-2 gap-6">
            {[
              { label: 'Legal Name',  value: vendor.legalName,        Icon: Building2 },
              { label: 'Trade Name',  value: vendor.tradeName ?? '—', Icon: Store     },
              { label: 'NPWP',        value: vendor.npwp    ?? '—',   Icon: Hash      },
              { label: 'SIUP',        value: vendor.siup    ?? '—',   Icon: FileCheck },
              { label: 'NIB',         value: vendor.nib     ?? '—',   Icon: Hash      },
              { label: 'Vendor Type', value: vendor.vendorType,       Icon: Tag       },
            ].map(({ label, value, Icon }) => (
              <div key={label}>
                <p className="flex items-center text-xs text-slate-500 mb-0.5">
                  <Icon className="h-3.5 w-3.5 mr-1 text-slate-400" />
                  {label}
                </p>
                <p className="text-sm font-medium text-slate-800">{value}</p>
              </div>
            ))}
          </div>

          {addressLine && (
            <div className="bg-slate-50 rounded-lg p-4">
              <p className="flex items-center text-xs text-slate-500 mb-1">
                <MapPin className="h-3.5 w-3.5 mr-1 text-slate-400" />
                Address
              </p>
              <p className="text-sm font-medium text-slate-800">{addressLine}</p>
            </div>
          )}

          {/* Tax Settings */}
          <div className="bg-slate-50 rounded-lg p-4">
            <div className="flex items-center justify-between mb-2">
              <p className="text-xs font-semibold text-slate-600 uppercase tracking-wide">Indonesian Tax</p>
              {isMasterData && (
                <button
                  onClick={openTaxModal}
                  className="text-xs text-blue-600 hover:text-blue-800 font-medium"
                >
                  Edit
                </button>
              )}
            </div>
            <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
              <div>
                <p className="text-xs text-slate-500 mb-0.5">PKP Status</p>
                <span className={`inline-flex items-center px-2 py-0.5 rounded-full text-xs font-medium ${
                  vendor.isPkp ? 'bg-emerald-50 text-emerald-700' : 'bg-slate-100 text-slate-500'
                }`}>
                  {vendor.isPkp ? 'PKP (VAT-registered)' : 'Non-PKP'}
                </span>
              </div>
              <div>
                <p className="text-xs text-slate-500 mb-0.5">PPh Rate</p>
                <p className="text-sm font-medium text-slate-800">
                  {vendor.pphRate != null ? `${vendor.pphRate}%` : '—'}
                </p>
              </div>
            </div>
          </div>

          {vendor.isBlacklisted && (
            <div className="bg-red-50 rounded-lg p-4">
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
                        <Mail className="h-3 w-3 mr-1 text-slate-400" />{c.email}
                      </span>
                    )}
                    {c.phone && (
                      <span className="flex items-center text-xs text-slate-500">
                        <Phone className="h-3 w-3 mr-1 text-slate-400" />{c.phone}
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
                  {d.expiredAt && <p className="text-xs text-slate-400 mt-1">Exp: {fmtDate(d.expiredAt)}</p>}
                </div>
                <div className="flex items-center gap-1.5 ml-2">
                  <Button variant="ghost" size="sm" className="text-slate-500 hover:text-blue-600"
                    onClick={() => handlePreview(d.id, d.fileName)}>
                    <Eye className="h-3.5 w-3.5 mr-1" /> Preview
                  </Button>
                  <Button variant="ghost" size="sm" className="text-slate-500 hover:text-emerald-600"
                    onClick={() => handleDownload(d.id, d.fileName)}>
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
            <p className="text-xs text-slate-500">Approved supply categories for this vendor</p>
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
                  <div className="flex items-center gap-2">
                    <p className="text-sm font-medium text-slate-900">
                      {categoryMap.get(cap.materialCategoryId) ?? cap.materialCategoryId}
                    </p>
                    {cap.isExpired && (
                      <span className="inline-flex items-center gap-0.5 text-xs bg-red-50 text-red-600 px-1.5 py-0.5 rounded-full">
                        <AlertTriangle className="h-3 w-3" /> Expired
                      </span>
                    )}
                    {!cap.isExpired && cap.expiryDate && (
                      <span className="inline-flex items-center gap-0.5 text-xs bg-emerald-50 text-emerald-600 px-1.5 py-0.5 rounded-full">
                        <CheckCircle2 className="h-3 w-3" /> Valid
                      </span>
                    )}
                  </div>
                  <div className="flex flex-wrap gap-4 mt-1 text-xs text-slate-500">
                    {cap.minOrderQty != null && (
                      <span>Min: <strong>{cap.minOrderQty}{cap.uom ? ` ${cap.uom}` : ''}</strong></span>
                    )}
                    {cap.maxOrderQty != null && (
                      <span>Max: <strong>{cap.maxOrderQty}{cap.uom ? ` ${cap.uom}` : ''}</strong></span>
                    )}
                    {cap.leadTimeDays != null && <span>Lead: <strong>{cap.leadTimeDays}d</strong></span>}
                    {cap.expiryDate && <span>Exp: <strong>{fmtDate(cap.expiryDate)}</strong></span>}
                    {cap.notes && <span className="italic">{cap.notes}</span>}
                  </div>
                </div>
                <div className="flex items-center gap-1">
                  <Button variant="ghost" size="icon" className="h-7 w-7 text-slate-400 hover:text-blue-600"
                    onClick={() => openEditCap(cap)}>
                    <Pencil className="h-3.5 w-3.5" />
                  </Button>
                  <Button variant="ghost" size="icon" className="h-7 w-7 text-slate-400 hover:text-red-500"
                    onClick={() => deleteCapMutation.mutate(cap.id)}
                    disabled={deleteCapMutation.isPending}>
                    <Trash2 className="h-3.5 w-3.5" />
                  </Button>
                </div>
              </div>
            ))
          )}
        </div>
      )}

      {/* ── Bank Accounts tab ── */}
      {activeTab === 'Bank Accounts' && (
        <div className="space-y-3">
          <div className="flex items-center justify-between">
            <p className="text-xs text-slate-500">Registered bank accounts for payment disbursement</p>
            <Button size="sm" onClick={openAddBank}>
              <Plus className="h-3.5 w-3.5 mr-1" /> Add Account
            </Button>
          </div>

          {vendor.bankAccounts.length === 0 ? (
            <div className="flex flex-col items-center justify-center py-12 text-slate-400">
              <Landmark className="h-10 w-10 mb-3" />
              <p className="text-sm">No bank accounts registered yet.</p>
            </div>
          ) : (
            vendor.bankAccounts.map((b) => (
              <div key={b.id} className="flex items-start gap-4 bg-white rounded-xl border border-slate-100 p-4">
                <div className="w-8 h-8 rounded-full bg-emerald-50 flex items-center justify-center text-emerald-600 flex-shrink-0">
                  <CreditCard className="h-4 w-4" />
                </div>
                <div className="flex-1">
                  <div className="flex items-center gap-2">
                    <p className="text-sm font-medium text-slate-900">{b.bankName}</p>
                    {b.isDefault && (
                      <span className="text-xs bg-blue-50 text-blue-600 px-1.5 py-0.5 rounded-full">Default</span>
                    )}
                    <span className="text-xs bg-slate-100 text-slate-500 px-1.5 py-0.5 rounded-full">{b.currency}</span>
                  </div>
                  <p className="text-sm text-slate-700 mt-0.5">{b.accountNumber}</p>
                  <p className="text-xs text-slate-500">a/n {b.accountName}</p>
                  {b.branchName && <p className="text-xs text-slate-400">{b.branchName}</p>}
                  {b.notes && <p className="text-xs text-slate-400 italic mt-0.5">{b.notes}</p>}
                </div>
                <div className="flex items-center gap-1">
                  <Button variant="ghost" size="icon" className="h-7 w-7 text-slate-400 hover:text-blue-600"
                    onClick={() => openEditBank(b)}>
                    <Pencil className="h-3.5 w-3.5" />
                  </Button>
                  <Button variant="ghost" size="icon" className="h-7 w-7 text-slate-400 hover:text-red-500"
                    onClick={() => deleteBankMutation.mutate(b.id)}
                    disabled={deleteBankMutation.isPending}>
                    <Trash2 className="h-3.5 w-3.5" />
                  </Button>
                </div>
              </div>
            ))
          )}
        </div>
      )}

      {/* ── Score History tab ── */}
      {activeTab === 'Score History' && (
        <div>
          {scoreHistory.length === 0 ? (
            <div className="text-center py-12 text-sm text-slate-400">No score records found.</div>
          ) : (
            <div className="space-y-3">
              {scoreHistory.map((s) => (
                <div key={s.id} className="bg-white rounded-xl border border-slate-100 p-5">
                  <div className="flex items-center justify-between mb-4">
                    <div className="flex items-center gap-2">
                      <TrendingUp className="h-4 w-4 text-slate-400" />
                      <span className="font-semibold text-slate-800">
                        Q{s.periodQuarter} {s.periodYear}
                      </span>
                      {s.tier && (
                        <span className="text-xs bg-indigo-50 text-indigo-600 px-2 py-0.5 rounded-full font-medium">
                          {s.tier}
                        </span>
                      )}
                    </div>
                    <div className="text-right">
                      <p className="text-xs text-slate-400">Total Score</p>
                      <p className={`text-xl font-bold ${
                        s.totalScore != null && s.totalScore >= 80 ? 'text-emerald-600'
                        : s.totalScore != null && s.totalScore >= 60 ? 'text-amber-500'
                        : 'text-red-500'
                      }`}>
                        {s.totalScore != null ? s.totalScore.toFixed(1) : '—'}
                      </p>
                    </div>
                  </div>
                  <div className="grid grid-cols-2 sm:grid-cols-5 gap-3">
                    {[
                      { label: 'Delivery',  value: s.deliveryScore  },
                      { label: 'Quality',   value: s.qualityScore   },
                      { label: 'Price',     value: s.priceScore     },
                      { label: 'Response',  value: s.responseScore  },
                      { label: 'Documents', value: s.docScore       },
                    ].map(({ label, value }) => (
                      <div key={label} className="bg-slate-50 rounded-lg p-3 text-center">
                        <p className="text-xs text-slate-400 mb-1">{label}</p>
                        <p className="text-sm font-semibold text-slate-700">
                          {value != null ? value.toFixed(1) : '—'}
                        </p>
                      </div>
                    ))}
                  </div>
                  {s.notes && <p className="text-xs text-slate-400 mt-3 italic">{s.notes}</p>}
                  <p className="text-xs text-slate-300 mt-2">Calculated: {fmtDateTime(s.calculatedAt)}</p>
                </div>
              ))}
            </div>
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

      {/* ── Tax Settings Modal ── */}
      <Dialog open={taxModalOpen} onOpenChange={(v) => { if (!v) setTaxModalOpen(false); }}>
        <DialogContent className="max-w-sm">
          <DialogHeader>
            <DialogTitle>Edit Tax Settings</DialogTitle>
          </DialogHeader>
          <form onSubmit={(e) => { e.preventDefault(); updateTaxMutation.mutate(); }} className="space-y-4 mt-2">
            <div className="flex items-center gap-3">
              <input
                type="checkbox"
                id="taxIsPkp"
                checked={taxIsPkp}
                onChange={(e) => setTaxIsPkp(e.target.checked)}
                className="h-4 w-4 rounded border-gray-300 text-blue-600 focus:ring-blue-500"
              />
              <label htmlFor="taxIsPkp" className="text-sm font-medium">
                PKP (Pengusaha Kena Pajak) — VAT-registered vendor
              </label>
            </div>
            <div>
              <label className="block text-sm font-medium mb-1">PPh Withholding Rate (%)</label>
              <input
                type="number" min="0" max="100" step="0.01"
                className={inputCls}
                value={taxPphRate}
                onChange={(e) => setTaxPphRate(e.target.value)}
                placeholder="e.g. 2.00"
              />
              <p className="text-xs text-slate-400 mt-1">Leave blank if no PPh withholding applies.</p>
            </div>
            <div className="flex justify-end gap-2 pt-1">
              <Button type="button" variant="outline" onClick={() => setTaxModalOpen(false)}>Cancel</Button>
              <Button type="submit" disabled={updateTaxMutation.isPending}>
                {updateTaxMutation.isPending ? 'Saving…' : 'Save'}
              </Button>
            </div>
          </form>
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
            <div className="grid grid-cols-3 gap-3">
              <div>
                <label className="block text-sm font-medium mb-1">Min Qty</label>
                <input type="number" min="0" step="0.01" className={inputCls}
                  value={capForm.minOrderQty}
                  onChange={(e) => setCapForm(f => ({ ...f, minOrderQty: e.target.value }))}
                  placeholder="e.g. 10" />
              </div>
              <div>
                <label className="block text-sm font-medium mb-1">Max Qty</label>
                <input type="number" min="0" step="0.01" className={inputCls}
                  value={capForm.maxOrderQty}
                  onChange={(e) => setCapForm(f => ({ ...f, maxOrderQty: e.target.value }))}
                  placeholder="e.g. 1000" />
              </div>
              <div>
                <label className="block text-sm font-medium mb-1">UoM</label>
                <select className={inputCls} value={capForm.uom}
                  onChange={(e) => setCapForm(f => ({ ...f, uom: e.target.value }))}>
                  <option value="">—</option>
                  {UOM_OPTIONS.map(u => <option key={u} value={u}>{u}</option>)}
                </select>
              </div>
            </div>
            <div>
              <label className="block text-sm font-medium mb-1">Lead Time (days)</label>
              <input type="number" min="0" step="1" className={inputCls}
                value={capForm.leadTimeDays}
                onChange={(e) => setCapForm(f => ({ ...f, leadTimeDays: e.target.value }))}
                placeholder="e.g. 7" />
            </div>
            <div className="grid grid-cols-2 gap-3">
              <div>
                <label className="block text-sm font-medium mb-1">Effective Date</label>
                <input type="date" className={inputCls}
                  value={capForm.effectiveDate}
                  onChange={(e) => setCapForm(f => ({ ...f, effectiveDate: e.target.value }))} />
              </div>
              <div>
                <label className="block text-sm font-medium mb-1">Expiry Date</label>
                <input type="date" className={inputCls}
                  value={capForm.expiryDate}
                  onChange={(e) => setCapForm(f => ({ ...f, expiryDate: e.target.value }))} />
              </div>
            </div>
            <div>
              <label className="block text-sm font-medium mb-1">Notes</label>
              <textarea rows={2} className={inputCls}
                value={capForm.notes}
                onChange={(e) => setCapForm(f => ({ ...f, notes: e.target.value }))}
                placeholder="Optional notes..." />
            </div>
            <div className="flex justify-end gap-2 pt-1">
              <Button type="button" variant="outline"
                onClick={() => { setCapModalOpen(false); setEditingCapId(null); setCapForm(emptyCapForm()); }}>
                Cancel
              </Button>
              <Button type="submit"
                disabled={addCapMutation.isPending || updateCapMutation.isPending || (!editingCapId && !capForm.materialCategoryId)}>
                {editingCapId ? 'Update' : 'Add'}
              </Button>
            </div>
          </form>
        </DialogContent>
      </Dialog>

      {/* ── Bank Account Add/Edit Modal ── */}
      <Dialog open={bankModalOpen} onOpenChange={(v) => { if (!v) { setBankModalOpen(false); setEditingBankId(null); setBankForm(emptyBankForm()); } }}>
        <DialogContent className="max-w-md">
          <DialogHeader>
            <DialogTitle>{editingBankId ? 'Edit Bank Account' : 'Add Bank Account'}</DialogTitle>
          </DialogHeader>
          <form onSubmit={handleBankSubmit} className="space-y-4 mt-2">
            <div className="grid grid-cols-2 gap-3">
              <div>
                <label className="block text-sm font-medium mb-1">Bank Name <span className="text-red-500">*</span></label>
                <input className={inputCls} required placeholder="e.g. BCA" value={bankForm.bankName}
                  onChange={(e) => setBankForm(f => ({ ...f, bankName: e.target.value }))} />
              </div>
              <div>
                <label className="block text-sm font-medium mb-1">Currency</label>
                <select className={inputCls} value={bankForm.currency}
                  onChange={(e) => setBankForm(f => ({ ...f, currency: e.target.value }))}>
                  {CURRENCY_OPTIONS.map(c => <option key={c} value={c}>{c}</option>)}
                </select>
              </div>
            </div>
            <div>
              <label className="block text-sm font-medium mb-1">Account Number <span className="text-red-500">*</span></label>
              <input className={inputCls} required placeholder="e.g. 1234567890" value={bankForm.accountNumber}
                onChange={(e) => setBankForm(f => ({ ...f, accountNumber: e.target.value }))} />
            </div>
            <div>
              <label className="block text-sm font-medium mb-1">Account Name <span className="text-red-500">*</span></label>
              <input className={inputCls} required placeholder="Account holder name" value={bankForm.accountName}
                onChange={(e) => setBankForm(f => ({ ...f, accountName: e.target.value }))} />
            </div>
            <div>
              <label className="block text-sm font-medium mb-1">Branch Name</label>
              <input className={inputCls} placeholder="e.g. KCP Sudirman" value={bankForm.branchName}
                onChange={(e) => setBankForm(f => ({ ...f, branchName: e.target.value }))} />
            </div>
            <div className="flex items-center gap-2">
              <input type="checkbox" id="isDefault" checked={bankForm.isDefault}
                onChange={(e) => setBankForm(f => ({ ...f, isDefault: e.target.checked }))}
                className="h-4 w-4 rounded border-gray-300 text-blue-600 focus:ring-blue-500" />
              <label htmlFor="isDefault" className="text-sm font-medium">Set as default account</label>
            </div>
            <div>
              <label className="block text-sm font-medium mb-1">Notes</label>
              <textarea rows={2} className={inputCls} placeholder="Optional notes..."
                value={bankForm.notes}
                onChange={(e) => setBankForm(f => ({ ...f, notes: e.target.value }))} />
            </div>
            <div className="flex justify-end gap-2 pt-1">
              <Button type="button" variant="outline"
                onClick={() => { setBankModalOpen(false); setEditingBankId(null); setBankForm(emptyBankForm()); }}>
                Cancel
              </Button>
              <Button type="submit"
                disabled={addBankMutation.isPending || updateBankMutation.isPending}>
                {editingBankId ? 'Update' : 'Add'}
              </Button>
            </div>
          </form>
        </DialogContent>
      </Dialog>
    </div>
  );
}
