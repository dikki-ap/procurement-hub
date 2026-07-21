import { useRef, useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { Building2, Mail, Phone, MapPin, Upload, Tag, Hash } from 'lucide-react';
import { toast } from 'sonner';
import { Button } from '@/components/ui/button';
import { companyApi, type UpdateCompanyRequest } from '../api/companyApi';
import { extractApiError } from '@/shared/lib/apiError';

const inputCls =
  'w-full rounded-md border border-input bg-transparent px-3 py-2 text-sm shadow-sm transition-colors placeholder:text-muted-foreground focus-visible:outline-none focus-visible:ring-1 focus-visible:ring-ring disabled:cursor-not-allowed disabled:opacity-50';

const COMPANY_TYPES = ['Manufacturer', 'Distributor', 'Service Provider', 'Trading', 'Government', 'NGO', 'Other'];

export default function CompanyProfilePage() {
  const qc = useQueryClient();
  const fileRef = useRef<HTMLInputElement>(null);
  const [form, setForm] = useState<UpdateCompanyRequest | null>(null);
  const [editing, setEditing] = useState(false);

  const { data: company, isLoading } = useQuery({
    queryKey: ['company-profile'],
    queryFn: companyApi.get,
    onSuccess: (data) => {
      setForm({
        name:    data.name,
        type:    data.type,
        address: data.address,
        phone:   data.phone,
        email:   data.email,
      });
    },
  } as any);

  const updateMut = useMutation({
    mutationFn: () => companyApi.update(form!),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['company-profile'] });
      toast.success('Company profile updated');
      setEditing(false);
    },
    onError: (e: unknown) => toast.error(extractApiError(e, 'Update failed')),
  });

  const logoMut = useMutation({
    mutationFn: (file: File) => companyApi.uploadLogo(file),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['company-profile'] });
      toast.success('Logo updated');
    },
    onError: (e: unknown) => toast.error(extractApiError(e, 'Logo upload failed')),
  });

  const handleLogoChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0];
    if (file) logoMut.mutate(file);
    e.target.value = '';
  };

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    if (form) updateMut.mutate();
  };

  const handleCancel = () => {
    if (company) {
      setForm({ name: company.name, type: company.type, address: company.address, phone: company.phone, email: company.email });
    }
    setEditing(false);
  };

  if (isLoading || !company || !form) {
    return (
      <div className="animate-pulse space-y-4">
        <div className="h-8 bg-slate-100 rounded w-1/3" />
        <div className="h-48 bg-slate-100 rounded" />
      </div>
    );
  }

  return (
    <div>
      <div className="flex flex-wrap items-center justify-between gap-3 mb-6">
        <div className="flex items-center gap-2">
          <Building2 className="h-5 w-5 text-muted-foreground flex-shrink-0" />
          <div>
            <h1 className="text-xl sm:text-2xl font-semibold">Company Profile</h1>
            <p className="text-sm text-muted-foreground">Manage your organization's basic information</p>
          </div>
        </div>
        {!editing && (
          <Button onClick={() => setEditing(true)} size="sm">Edit Profile</Button>
        )}
      </div>

      {/* Logo card */}
      <div className="bg-white rounded-xl border border-slate-100 p-6 mb-6">
        <div className="flex items-center gap-6">
          <div className="w-20 h-20 rounded-xl bg-slate-100 flex items-center justify-center overflow-hidden flex-shrink-0">
            {company.logoUrl ? (
              <img src={`/api/v1/company/logo-preview`} alt="Company logo" className="w-full h-full object-contain" />
            ) : (
              <Building2 className="h-8 w-8 text-slate-400" />
            )}
          </div>
          <div className="flex-1">
            <h2 className="text-lg font-semibold text-slate-800">{company.name}</h2>
            <p className="text-sm text-slate-500">{company.code} · {company.type}</p>
          </div>
          <div>
            <input
              ref={fileRef}
              type="file"
              accept="image/jpeg,image/png,image/webp"
              className="hidden"
              onChange={handleLogoChange}
            />
            <Button
              variant="outline"
              size="sm"
              onClick={() => fileRef.current?.click()}
              disabled={logoMut.isPending}
            >
              <Upload className="h-3.5 w-3.5 mr-1.5" />
              {logoMut.isPending ? 'Uploading…' : 'Upload Logo'}
            </Button>
            <p className="text-xs text-slate-400 mt-1">JPEG, PNG, WebP · max 2 MB</p>
          </div>
        </div>
      </div>

      {/* Details form */}
      <div className="bg-white rounded-xl border border-slate-100 p-6">
        <form onSubmit={handleSubmit} className="space-y-5">
          <div className="grid grid-cols-2 gap-5">
            <div>
              <label className="block text-sm font-medium mb-1">
                <Building2 className="inline h-3.5 w-3.5 mr-1 text-slate-400" />
                Company Name {editing && <span className="text-red-500">*</span>}
              </label>
              {editing ? (
                <input
                  required
                  className={inputCls}
                  value={form.name}
                  onChange={(e) => setForm(f => f ? { ...f, name: e.target.value } : f)}
                />
              ) : (
                <p className="text-sm font-medium text-slate-800">{company.name}</p>
              )}
            </div>

            <div>
              <label className="block text-sm font-medium mb-1">
                <Hash className="inline h-3.5 w-3.5 mr-1 text-slate-400" />
                Company Code
              </label>
              <p className="text-sm font-medium text-slate-800">{company.code}</p>
              {editing && <p className="text-xs text-slate-400 mt-0.5">Code cannot be changed</p>}
            </div>

            <div>
              <label className="block text-sm font-medium mb-1">
                <Tag className="inline h-3.5 w-3.5 mr-1 text-slate-400" />
                Company Type {editing && <span className="text-red-500">*</span>}
              </label>
              {editing ? (
                <select
                  required
                  className={inputCls}
                  value={form.type}
                  onChange={(e) => setForm(f => f ? { ...f, type: e.target.value } : f)}
                >
                  {COMPANY_TYPES.map(t => <option key={t} value={t}>{t}</option>)}
                </select>
              ) : (
                <p className="text-sm font-medium text-slate-800">{company.type}</p>
              )}
            </div>

            <div>
              <label className="block text-sm font-medium mb-1">
                <Mail className="inline h-3.5 w-3.5 mr-1 text-slate-400" />
                Email
              </label>
              {editing ? (
                <input
                  type="email"
                  className={inputCls}
                  value={form.email ?? ''}
                  onChange={(e) => setForm(f => f ? { ...f, email: e.target.value || null } : f)}
                  placeholder="company@example.com"
                />
              ) : (
                <p className="text-sm font-medium text-slate-800">{company.email ?? '—'}</p>
              )}
            </div>

            <div>
              <label className="block text-sm font-medium mb-1">
                <Phone className="inline h-3.5 w-3.5 mr-1 text-slate-400" />
                Phone
              </label>
              {editing ? (
                <input
                  className={inputCls}
                  value={form.phone ?? ''}
                  onChange={(e) => setForm(f => f ? { ...f, phone: e.target.value || null } : f)}
                  placeholder="+62 21 xxx xxxx"
                />
              ) : (
                <p className="text-sm font-medium text-slate-800">{company.phone ?? '—'}</p>
              )}
            </div>
          </div>

          <div>
            <label className="block text-sm font-medium mb-1">
              <MapPin className="inline h-3.5 w-3.5 mr-1 text-slate-400" />
              Address
            </label>
            {editing ? (
              <textarea
                rows={2}
                className={inputCls}
                value={form.address ?? ''}
                onChange={(e) => setForm(f => f ? { ...f, address: e.target.value || null } : f)}
                placeholder="Full company address"
              />
            ) : (
              <p className="text-sm font-medium text-slate-800">{company.address ?? '—'}</p>
            )}
          </div>

          {editing && (
            <div className="flex justify-end gap-2 pt-1 border-t border-slate-100">
              <Button type="button" variant="outline" onClick={handleCancel}>Cancel</Button>
              <Button type="submit" disabled={updateMut.isPending}>
                {updateMut.isPending ? 'Saving…' : 'Save Changes'}
              </Button>
            </div>
          )}
        </form>
      </div>
    </div>
  );
}
