import { useState } from 'react';
import { useParams } from 'react-router-dom';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import {
  User, Building2, Store, Hash, FileCheck, Tag, Mail, Phone,
  Package, MapPin, CreditCard, TrendingUp, Pencil, X, Save,
} from 'lucide-react';
import { vendorPortalApi, type VendorStatus, type VendorDetailDto } from '@/features/vendors/api/vendorApi';
import { TierBadge, ScoreDisplay } from '@/features/vendors/components/VendorBadges';
import { fmtDate } from '@/shared/lib/date';
import { toast } from 'sonner';

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

type ProfileFormData = {
  tradeName: string;
  npwp: string;
  siup: string;
  nib: string;
  address: string;
  city: string;
  province: string;
  postalCode: string;
  country: string;
};

function toFormData(v: VendorDetailDto): ProfileFormData {
  return {
    tradeName:  v.tradeName  ?? '',
    npwp:       v.npwp       ?? '',
    siup:       v.siup       ?? '',
    nib:        v.nib        ?? '',
    address:    v.address    ?? '',
    city:       v.city       ?? '',
    province:   v.province   ?? '',
    postalCode: v.postalCode ?? '',
    country:    v.country    ?? '',
  };
}

function EditProfileModal({
  vendor,
  onClose,
}: {
  vendor: VendorDetailDto;
  onClose: () => void;
}) {
  const qc = useQueryClient();
  const { vendorId } = useParams<{ vendorId: string }>();
  const [form, setForm] = useState<ProfileFormData>(toFormData(vendor));

  const mutation = useMutation({
    mutationFn: () =>
      vendorPortalApi.updateProfile(vendorId!, {
        tradeName:  form.tradeName  || null,
        npwp:       form.npwp       || null,
        siup:       form.siup       || null,
        nib:        form.nib        || null,
        address:    form.address    || null,
        city:       form.city       || null,
        province:   form.province   || null,
        postalCode: form.postalCode || null,
        country:    form.country    || null,
      }),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['vendor-portal', 'profile', vendorId] });
      toast.success('Profile updated successfully.');
      onClose();
    },
    onError: () => toast.error('Failed to update profile.'),
  });

  const set = (field: keyof ProfileFormData) => (e: React.ChangeEvent<HTMLInputElement>) =>
    setForm((prev) => ({ ...prev, [field]: e.target.value }));

  const inputCls = 'w-full border border-slate-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-primary/30';

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/40 p-4">
      <div className="bg-white rounded-xl shadow-xl w-full max-w-lg">
        <div className="flex items-center justify-between px-6 py-4 border-b border-slate-100">
          <h2 className="text-base font-semibold text-slate-800">Edit Profile</h2>
          <button onClick={onClose} className="text-slate-400 hover:text-slate-600">
            <X className="h-4 w-4" />
          </button>
        </div>
        <div className="px-6 py-5 space-y-4 max-h-[70vh] overflow-y-auto">
          <div className="grid grid-cols-2 gap-4">
            <div>
              <label className="block text-xs text-slate-500 mb-1">Trade Name</label>
              <input className={inputCls} value={form.tradeName} onChange={set('tradeName')} placeholder="Trade name" />
            </div>
            <div>
              <label className="block text-xs text-slate-500 mb-1">NPWP</label>
              <input className={inputCls} value={form.npwp} onChange={set('npwp')} placeholder="Tax ID" />
            </div>
            <div>
              <label className="block text-xs text-slate-500 mb-1">SIUP</label>
              <input className={inputCls} value={form.siup} onChange={set('siup')} placeholder="Business licence" />
            </div>
            <div>
              <label className="block text-xs text-slate-500 mb-1">NIB</label>
              <input className={inputCls} value={form.nib} onChange={set('nib')} placeholder="Business registration" />
            </div>
          </div>
          <div>
            <label className="block text-xs text-slate-500 mb-1">Address</label>
            <input className={inputCls} value={form.address} onChange={set('address')} placeholder="Street address" />
          </div>
          <div className="grid grid-cols-2 gap-4">
            <div>
              <label className="block text-xs text-slate-500 mb-1">City</label>
              <input className={inputCls} value={form.city} onChange={set('city')} placeholder="City" />
            </div>
            <div>
              <label className="block text-xs text-slate-500 mb-1">Province</label>
              <input className={inputCls} value={form.province} onChange={set('province')} placeholder="Province / State" />
            </div>
            <div>
              <label className="block text-xs text-slate-500 mb-1">Postal Code</label>
              <input className={inputCls} value={form.postalCode} onChange={set('postalCode')} placeholder="Postal code" />
            </div>
            <div>
              <label className="block text-xs text-slate-500 mb-1">Country</label>
              <input className={inputCls} value={form.country} onChange={set('country')} placeholder="Country" />
            </div>
          </div>
        </div>
        <div className="flex justify-end gap-2 px-6 py-4 border-t border-slate-100">
          <button
            onClick={onClose}
            className="px-4 py-2 text-sm text-slate-600 border border-slate-200 rounded-lg hover:bg-slate-50"
          >
            Cancel
          </button>
          <button
            onClick={() => mutation.mutate()}
            disabled={mutation.isPending}
            className="flex items-center gap-1.5 px-4 py-2 text-sm bg-primary text-white rounded-lg hover:bg-primary/90 disabled:opacity-60"
          >
            <Save className="h-3.5 w-3.5" />
            {mutation.isPending ? 'Saving…' : 'Save Changes'}
          </button>
        </div>
      </div>
    </div>
  );
}

export default function VendorPortalProfilePage() {
  const { vendorId } = useParams<{ vendorId: string }>();
  const [editOpen, setEditOpen] = useState(false);

  const { data: vendor, isLoading } = useQuery({
    queryKey: ['vendor-portal', 'profile', vendorId],
    queryFn: () => vendorPortalApi.getProfile(vendorId!),
    enabled: !!vendorId,
  });

  const { data: scoreHistory = [] } = useQuery({
    queryKey: ['vendor-portal', 'scores', vendorId],
    queryFn: () => vendorPortalApi.getScoreHistory(vendorId!),
    enabled: !!vendorId,
  });

  if (isLoading) {
    return (
      <div className="animate-pulse space-y-4">
        <div className="h-8 bg-slate-100 rounded w-1/3" />
        <div className="h-48 bg-slate-100 rounded" />
      </div>
    );
  }

  if (!vendor) return null;

  const addressLine = [vendor.address, vendor.city, vendor.province, vendor.postalCode, vendor.country]
    .filter(Boolean).join(', ');

  const infoFields = [
    { label: 'Legal Name',  value: vendor.legalName,        Icon: Building2 },
    { label: 'Trade Name',  value: vendor.tradeName ?? '—', Icon: Store     },
    { label: 'NPWP',        value: vendor.npwp    ?? '—',   Icon: Hash      },
    { label: 'SIUP',        value: vendor.siup    ?? '—',   Icon: FileCheck },
    { label: 'NIB',         value: vendor.nib     ?? '—',   Icon: Hash      },
  ];

  return (
    <div>
      {editOpen && vendor && (
        <EditProfileModal vendor={vendor} onClose={() => setEditOpen(false)} />
      )}

      <div className="flex flex-wrap items-center justify-between gap-3 mb-6">
        <div className="flex items-center gap-2">
          <Building2 className="h-5 w-5 text-muted-foreground flex-shrink-0" />
          <div>
            <h1 className="text-xl sm:text-2xl font-semibold">Company Profile</h1>
            <p className="text-sm text-muted-foreground">{vendor.vendorCode}</p>
          </div>
        </div>
        <div className="flex items-center gap-2">
          <StatusBadge status={vendor.status} />
          <button
            onClick={() => setEditOpen(true)}
            className="flex items-center gap-1.5 px-3 py-1.5 text-xs border border-slate-200 rounded-lg text-slate-600 hover:bg-slate-50"
          >
            <Pencil className="h-3.5 w-3.5" />
            Edit Profile
          </button>
        </div>
      </div>

      {/* Score / Tier / Type cards */}
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
          <p className="text-xs text-slate-500 mb-1.5">Type</p>
          <div className="flex items-center gap-1.5">
            <Tag className="h-3.5 w-3.5 text-slate-400" />
            <p className="text-sm font-medium text-slate-800">{vendor.vendorType}</p>
          </div>
        </div>
      </div>

      {/* Company Details */}
      <div className="bg-white rounded-xl border border-slate-100 p-6 mb-6">
        <h2 className="text-sm font-semibold text-slate-700 mb-4">Company Details</h2>
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
        </div>
        {addressLine && (
          <div className="mt-4 pt-4 border-t border-slate-100">
            <p className="flex items-center text-xs text-slate-500 mb-1">
              <MapPin className="h-3.5 w-3.5 mr-1 text-slate-400" />
              Address
            </p>
            <p className="text-sm font-medium text-slate-800">{addressLine}</p>
          </div>
        )}
      </div>

      {/* Contacts */}
      <div className="bg-white rounded-xl border border-slate-100 p-6 mb-6">
        <h2 className="text-sm font-semibold text-slate-700 mb-4">Contacts</h2>
        <div className="space-y-3">
          {vendor.contacts.length === 0 ? (
            <p className="text-sm text-slate-500">No contacts on record.</p>
          ) : (
            vendor.contacts.map((c) => (
              <div key={c.id} className="flex items-start gap-3">
                <div className="w-8 h-8 rounded-full bg-primary/10 flex items-center justify-center flex-shrink-0">
                  <User className="h-4 w-4 text-primary" />
                </div>
                <div>
                  <p className="text-sm font-medium text-slate-900">
                    {c.name}
                    {c.isPrimary && (
                      <span className="ml-2 text-xs bg-primary/10 text-primary px-1.5 py-0.5 rounded">Primary</span>
                    )}
                  </p>
                  {c.position && <p className="text-xs text-slate-500">{c.position}</p>}
                  <div className="flex flex-wrap gap-4 mt-1">
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
      </div>

      {/* Bank Accounts */}
      {vendor.bankAccounts.length > 0 && (
        <div className="bg-white rounded-xl border border-slate-100 p-6 mb-6">
          <h2 className="text-sm font-semibold text-slate-700 mb-4">Bank Accounts</h2>
          <div className="space-y-3">
            {vendor.bankAccounts.map((b) => (
              <div key={b.id} className="flex items-start gap-3 p-3 rounded-lg bg-slate-50">
                <div className="w-7 h-7 rounded-full bg-emerald-50 flex items-center justify-center flex-shrink-0">
                  <CreditCard className="h-3.5 w-3.5 text-emerald-600" />
                </div>
                <div className="flex-1">
                  <div className="flex items-center gap-2">
                    <p className="text-sm font-medium text-slate-800">{b.bankName}</p>
                    {b.isDefault && (
                      <span className="text-xs bg-blue-50 text-blue-600 px-1.5 py-0.5 rounded-full">Default</span>
                    )}
                    <span className="text-xs bg-slate-100 text-slate-500 px-1.5 py-0.5 rounded-full">{b.currency}</span>
                  </div>
                  <p className="text-sm text-slate-700">{b.accountNumber}</p>
                  <p className="text-xs text-slate-500">a/n {b.accountName}</p>
                  {b.branchName && <p className="text-xs text-slate-400">{b.branchName}</p>}
                </div>
              </div>
            ))}
          </div>
        </div>
      )}

      {/* Score History */}
      {scoreHistory.length > 0 && (
        <div className="bg-white rounded-xl border border-slate-100 p-6 mb-6">
          <h2 className="text-sm font-semibold text-slate-700 mb-1">Performance Score History</h2>
          <p className="text-xs text-slate-400 mb-4">Quarterly evaluation scores</p>
          <div className="space-y-3">
            {scoreHistory.map((s) => (
              <div key={s.id} className="p-3 rounded-lg bg-slate-50">
                <div className="flex items-center justify-between">
                  <div className="flex items-center gap-2">
                    <TrendingUp className="h-3.5 w-3.5 text-slate-400" />
                    <span className="text-sm font-medium text-slate-700">Q{s.periodQuarter} {s.periodYear}</span>
                    {s.tier && (
                      <span className="text-xs bg-indigo-50 text-indigo-600 px-1.5 py-0.5 rounded-full">{s.tier}</span>
                    )}
                  </div>
                  <span className={`text-lg font-bold ${
                    s.totalScore != null && s.totalScore >= 80 ? 'text-emerald-600'
                    : s.totalScore != null && s.totalScore >= 60 ? 'text-amber-500'
                    : 'text-red-500'
                  }`}>
                    {s.totalScore != null ? s.totalScore.toFixed(1) : '—'}
                  </span>
                </div>
                <div className="grid grid-cols-5 gap-2 mt-2">
                  {[
                    { label: 'Delivery',  value: s.deliveryScore  },
                    { label: 'Quality',   value: s.qualityScore   },
                    { label: 'Price',     value: s.priceScore     },
                    { label: 'Response',  value: s.responseScore  },
                    { label: 'Docs',      value: s.docScore       },
                  ].map(({ label, value }) => (
                    <div key={label} className="text-center">
                      <p className="text-xs text-slate-400">{label}</p>
                      <p className="text-xs font-semibold text-slate-600">{value != null ? value.toFixed(1) : '—'}</p>
                    </div>
                  ))}
                </div>
              </div>
            ))}
          </div>
        </div>
      )}

      {/* Capabilities (read-only) */}
      {vendor.capabilities.length > 0 && (
        <div className="bg-white rounded-xl border border-slate-100 p-6">
          <h2 className="text-sm font-semibold text-slate-700 mb-1">Approved Supply Categories</h2>
          <p className="text-xs text-slate-400 mb-4">Categories your company is qualified to supply</p>
          <div className="space-y-2">
            {vendor.capabilities.map((cap) => (
              <div key={cap.id} className="flex items-start gap-3 p-3 rounded-lg bg-slate-50">
                <div className="w-7 h-7 rounded-full bg-indigo-50 flex items-center justify-center flex-shrink-0">
                  <Package className="h-3.5 w-3.5 text-indigo-500" />
                </div>
                <div>
                  <div className="flex items-center gap-2">
                    <p className="text-sm font-medium text-slate-800">
                      {cap.materialCategoryName ?? cap.materialCategoryId}
                    </p>
                    {cap.isExpired && (
                      <span className="text-xs bg-red-50 text-red-600 px-1.5 py-0.5 rounded-full">Expired</span>
                    )}
                  </div>
                  <div className="flex flex-wrap gap-3 mt-0.5 text-xs text-slate-500">
                    {cap.minOrderQty != null && <span>Min Order: <strong>{cap.minOrderQty}{cap.uom ? ` ${cap.uom}` : ''}</strong></span>}
                    {cap.maxOrderQty != null && <span>Max Order: <strong>{cap.maxOrderQty}{cap.uom ? ` ${cap.uom}` : ''}</strong></span>}
                    {cap.leadTimeDays != null && <span>Lead Time: <strong>{cap.leadTimeDays}d</strong></span>}
                    {cap.expiryDate && <span>Exp: <strong>{fmtDate(cap.expiryDate)}</strong></span>}
                    {cap.notes && <span className="italic">{cap.notes}</span>}
                  </div>
                </div>
              </div>
            ))}
          </div>
        </div>
      )}
    </div>
  );
}
