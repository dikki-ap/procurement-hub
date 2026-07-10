import { useParams } from 'react-router-dom';
import { useQuery } from '@tanstack/react-query';
import { User, Building2 } from 'lucide-react';
import { vendorPortalApi, type VendorStatus } from '@/features/vendors/api/vendorApi';
import { TierBadge, ScoreDisplay } from '@/features/vendors/components/VendorBadges';

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

export default function VendorPortalProfilePage() {
  const { vendorId } = useParams<{ vendorId: string }>();

  const { data: vendor, isLoading } = useQuery({
    queryKey: ['vendor-portal', 'profile', vendorId],
    queryFn: () => vendorPortalApi.getProfile(vendorId!),
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

  return (
    <div>
      <div className="flex flex-wrap items-center justify-between gap-3 mb-6">
        <div className="flex items-center gap-2">
          <Building2 className="h-5 w-5 text-muted-foreground flex-shrink-0" />
          <div>
            <h1 className="text-xl sm:text-2xl font-semibold">Company Profile</h1>
            <p className="text-sm text-muted-foreground">{vendor.vendorCode}</p>
          </div>
        </div>
        <StatusBadge status={vendor.status} />
      </div>

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
          <p className="text-sm font-medium text-slate-800">{vendor.vendorType}</p>
        </div>
      </div>

      <div className="bg-white rounded-xl border border-slate-100 p-6 mb-6">
        <h2 className="text-sm font-semibold text-slate-700 mb-4">Company Details</h2>
        <div className="grid grid-cols-2 gap-6">
          {[
            { label: 'Legal Name',  value: vendor.legalName },
            { label: 'Trade Name',  value: vendor.tradeName ?? '—' },
            { label: 'NPWP',        value: vendor.npwp ?? '—' },
            { label: 'SIUP',        value: vendor.siup ?? '—' },
            { label: 'NIB',         value: vendor.nib ?? '—' },
          ].map(({ label, value }) => (
            <div key={label}>
              <p className="text-xs text-slate-500">{label}</p>
              <p className="text-sm font-medium text-slate-800 mt-0.5">{value}</p>
            </div>
          ))}
        </div>
      </div>

      <div className="bg-white rounded-xl border border-slate-100 p-6">
        <h2 className="text-sm font-semibold text-slate-700 mb-4">Contacts</h2>
        <div className="space-y-3">
          {vendor.contacts.map((c) => (
            <div key={c.id} className="flex items-center gap-3">
              <div className="w-8 h-8 rounded-full bg-primary/10 flex items-center justify-center">
                <User className="h-4 w-4 text-primary" />
              </div>
              <div>
                <p className="text-sm font-medium text-slate-900">
                  {c.name}
                  {c.isPrimary && (
                    <span className="ml-2 text-xs bg-primary/10 text-primary px-1.5 py-0.5 rounded">Primary</span>
                  )}
                </p>
                <p className="text-xs text-slate-500">{c.position} · {c.email}</p>
              </div>
            </div>
          ))}
        </div>
      </div>
    </div>
  );
}
