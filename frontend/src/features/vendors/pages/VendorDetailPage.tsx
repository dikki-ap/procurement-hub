import { useState } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { useQuery } from '@tanstack/react-query';
import { ArrowLeft, FileText, User } from 'lucide-react';
import { Button } from '@/components/ui/button';
import { vendorApi, type VendorStatus, type DocumentStatus } from '../api/vendorApi';
import { TierBadge, ScoreDisplay } from '../components/VendorBadges';
import { fmtDate, fmtDateTime } from '@/shared/lib/date';

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

const tabs = ['Info', 'Contacts', 'Documents', 'Capabilities'] as const;
type Tab = typeof tabs[number];

export default function VendorDetailPage() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const [activeTab, setActiveTab] = useState<Tab>('Info');

  const { data: vendor, isLoading } = useQuery({
    queryKey: ['vendor', id],
    queryFn: () => vendorApi.getById(id!),
    enabled: !!id,
  });

  if (isLoading) {
    return (
      <div className="animate-pulse space-y-4">
        <div className="h-8 bg-slate-100 rounded w-1/3" />
        <div className="h-32 bg-slate-100 rounded" />
      </div>
    );
  }

  if (!vendor) return null;

  return (
    <div>
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

      {/* Score card row */}
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
          <p className="text-sm font-semibold text-slate-900">
            {fmtDateTime(vendor.approvedAt)}
          </p>
        </div>
      </div>

      {/* Tabs */}
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

      {activeTab === 'Info' && (
        <div className="grid grid-cols-2 gap-6">
          {[
            { label: 'Legal Name', value: vendor.legalName },
            { label: 'Trade Name', value: vendor.tradeName ?? '—' },
            { label: 'NPWP', value: vendor.npwp ?? '—' },
            { label: 'SIUP', value: vendor.siup ?? '—' },
            { label: 'NIB', value: vendor.nib ?? '—' },
            { label: 'Vendor Type', value: vendor.vendorType },
          ].map(({ label, value }) => (
            <div key={label}>
              <p className="text-xs text-slate-500">{label}</p>
              <p className="text-sm font-medium text-slate-800 mt-0.5">{value}</p>
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
                <div>
                  <p className="text-sm font-medium text-slate-900">
                    {c.name}
                    {c.isPrimary && (
                      <span className="ml-2 text-xs bg-blue-50 text-blue-600 px-1.5 py-0.5 rounded">Primary</span>
                    )}
                  </p>
                  {c.position && <p className="text-xs text-slate-500">{c.position}</p>}
                  <div className="flex gap-3 mt-1 text-xs text-slate-500">
                    {c.email && <span>{c.email}</span>}
                    {c.phone && <span>{c.phone}</span>}
                  </div>
                </div>
              </div>
            ))
          )}
        </div>
      )}

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
                    <p className="text-xs text-slate-400 mt-1">
                      Exp: {fmtDate(d.expiredAt)}
                    </p>
                  )}
                </div>
              </div>
            ))
          )}
        </div>
      )}

      {activeTab === 'Capabilities' && (
        <div className="space-y-3">
          {vendor.capabilities.length === 0 ? (
            <p className="text-sm text-slate-500">No capabilities registered.</p>
          ) : (
            vendor.capabilities.map((cap) => (
              <div key={cap.id} className="bg-white rounded-xl border border-slate-100 p-4">
                <p className="text-sm font-medium text-slate-900">Category: {cap.materialCategoryId}</p>
                <div className="flex gap-6 mt-2 text-xs text-slate-500">
                  {cap.minOrderQty != null && <span>Min Order: {cap.minOrderQty}</span>}
                  {cap.leadTimeDays != null && <span>Lead Time: {cap.leadTimeDays}d</span>}
                  {cap.notes && <span>{cap.notes}</span>}
                </div>
              </div>
            ))
          )}
        </div>
      )}
    </div>
  );
}
