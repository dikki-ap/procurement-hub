import { useNavigate } from 'react-router-dom';
import { useMutation, useQueryClient } from '@tanstack/react-query';
import { ArrowLeft } from 'lucide-react';
import { toast } from 'sonner';
import { Button } from '@/components/ui/button';
import { vendorRegistrationApi, type VendorType } from '../api/vendorApi';
import { extractApiError } from '@/shared/lib/apiError';

const COMPANY_ID = '00000000-0000-0000-0000-000000000001';

const vendorTypes: VendorType[] = ['Manufacturer', 'Distributor', 'Trader'];

export default function VendorFormPage() {
  const navigate = useNavigate();
  const qc = useQueryClient();

  const mutation = useMutation({
    mutationFn: vendorRegistrationApi.register,
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['vendors'] });
      toast.success('Vendor registered successfully');
      navigate('/app/vendors');
    },
    onError: (error: unknown) => toast.error(extractApiError(error, 'Failed to register vendor')),
  });

  const handleSubmit = (e: React.FormEvent<HTMLFormElement>) => {
    e.preventDefault();
    const fd = new FormData(e.currentTarget);
    mutation.mutate({
      companyId:       COMPANY_ID,
      legalName:       fd.get('legalName') as string,
      tradeName:       (fd.get('tradeName') as string) || undefined,
      vendorType:      fd.get('vendorType') as VendorType,
      npwp:            (fd.get('npwp') as string) || undefined,
      siup:            (fd.get('siup') as string) || undefined,
      nib:             (fd.get('nib') as string) || undefined,
      contactName:     fd.get('contactName') as string,
      contactPosition: (fd.get('contactPosition') as string) || undefined,
      contactEmail:    fd.get('contactEmail') as string,
      contactPhone:    (fd.get('contactPhone') as string) || undefined,
    });
  };

  const inputCls =
    'w-full rounded-lg border border-slate-200 px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500/20 focus:border-blue-400';

  return (
    <div>
      <div className="flex items-center gap-3 mb-6">
        <Button variant="ghost" size="icon" onClick={() => navigate(-1)}>
          <ArrowLeft className="h-4 w-4" />
        </Button>
        <div>
          <h1 className="text-xl font-semibold text-slate-900">Add Vendor</h1>
          <p className="text-sm text-slate-500 mt-0.5">Register a new vendor into the system</p>
        </div>
      </div>

      <form onSubmit={handleSubmit} className="max-w-2xl space-y-8">
        {/* Company */}
        <div className="bg-white rounded-xl border border-slate-100 p-6">
          <h2 className="text-sm font-semibold text-slate-700 mb-4 uppercase tracking-wider">
            Company Information
          </h2>
          <div className="grid grid-cols-2 gap-4">
            <div>
              <label className="block text-sm font-medium text-slate-700 mb-1">
                Legal Name <span className="text-red-500">*</span>
              </label>
              <input name="legalName" required className={inputCls} />
            </div>
            <div>
              <label className="block text-sm font-medium text-slate-700 mb-1">Trade Name</label>
              <input name="tradeName" className={inputCls} />
            </div>
            <div>
              <label className="block text-sm font-medium text-slate-700 mb-1">
                Vendor Type <span className="text-red-500">*</span>
              </label>
              <select name="vendorType" required className={inputCls}>
                {vendorTypes.map((t) => (
                  <option key={t} value={t}>{t}</option>
                ))}
              </select>
            </div>
            <div>
              <label className="block text-sm font-medium text-slate-700 mb-1">NPWP</label>
              <input name="npwp" className={inputCls} />
            </div>
            <div>
              <label className="block text-sm font-medium text-slate-700 mb-1">SIUP</label>
              <input name="siup" className={inputCls} />
            </div>
            <div>
              <label className="block text-sm font-medium text-slate-700 mb-1">NIB</label>
              <input name="nib" className={inputCls} />
            </div>
          </div>
        </div>

        {/* Primary Contact */}
        <div className="bg-white rounded-xl border border-slate-100 p-6">
          <h2 className="text-sm font-semibold text-slate-700 mb-4 uppercase tracking-wider">
            Primary Contact
          </h2>
          <div className="grid grid-cols-2 gap-4">
            <div>
              <label className="block text-sm font-medium text-slate-700 mb-1">
                Name <span className="text-red-500">*</span>
              </label>
              <input name="contactName" required className={inputCls} />
            </div>
            <div>
              <label className="block text-sm font-medium text-slate-700 mb-1">Position</label>
              <input name="contactPosition" className={inputCls} />
            </div>
            <div>
              <label className="block text-sm font-medium text-slate-700 mb-1">
                Email <span className="text-red-500">*</span>
              </label>
              <input name="contactEmail" type="email" required className={inputCls} />
            </div>
            <div>
              <label className="block text-sm font-medium text-slate-700 mb-1">Phone</label>
              <input name="contactPhone" className={inputCls} />
            </div>
          </div>
        </div>

        <div className="flex gap-3">
          <Button type="submit" disabled={mutation.isPending}>
            {mutation.isPending ? 'Saving...' : 'Register Vendor'}
          </Button>
          <Button type="button" variant="outline" onClick={() => navigate(-1)}>
            Cancel
          </Button>
        </div>
      </form>
    </div>
  );
}
