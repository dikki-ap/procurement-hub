import { useParams } from 'react-router-dom';
import { useQuery } from '@tanstack/react-query';
import { User } from 'lucide-react';
import { vendorPortalApi, type VendorStatus } from '@/features/vendors/api/vendorApi';

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
      <div className="flex items-center justify-between mb-6">
        <div>
          <h1 className="text-xl font-semibold text-slate-900">Company Profile</h1>
          <p className="text-sm text-slate-500 mt-0.5">{vendor.vendorCode}</p>
        </div>
        <StatusBadge status={vendor.status} />
      </div>

      <div className="grid grid-cols-3 gap-4 mb-6">
        {[
          { label: 'Score', value: vendor.score.toFixed(1) },
          { label: 'Tier', value: vendor.tier },
          { label: 'Type', value: vendor.vendorType },
        ].map(({ label, value }) => (
          <div key={label} className="bg-white rounded-xl border border-slate-100 p-4">
            <p className="text-xs text-slate-500">{label}</p>
            <p className="text-lg font-semibold text-slate-900 mt-0.5">{value}</p>
          </div>
        ))}
      </div>

      <div className="bg-white rounded-xl border border-slate-100 p-6 mb-6">
        <h2 className="text-sm font-semibold text-slate-700 mb-4">Company Details</h2>
        <div className="grid grid-cols-2 gap-6">
          {[
            { label: 'Legal Name', value: vendor.legalName },
            { label: 'Trade Name', value: vendor.tradeName ?? '—' },
            { label: 'NPWP', value: vendor.npwp ?? '—' },
            { label: 'SIUP', value: vendor.siup ?? '—' },
            { label: 'NIB', value: vendor.nib ?? '—' },
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
              <div className="w-8 h-8 rounded-full bg-blue-100 flex items-center justify-center">
                <User className="h-4 w-4 text-blue-600" />
              </div>
              <div>
                <p className="text-sm font-medium text-slate-900">
                  {c.name}
                  {c.isPrimary && (
                    <span className="ml-2 text-xs bg-blue-50 text-blue-600 px-1.5 py-0.5 rounded">Primary</span>
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
