import { useState } from 'react';
import { useMutation } from '@tanstack/react-query';
import { CheckCircle2 } from 'lucide-react';
import { toast } from 'sonner';
import { Button } from '@/components/ui/button';
import { vendorRegistrationApi, type VendorType } from '@/features/vendors/api/vendorApi';
import { extractApiError } from '@/shared/lib/apiError';

const COMPANY_ID = '00000000-0000-0000-0000-000000000001'; // default company

const vendorTypes: VendorType[] = ['Manufacturer', 'Distributor', 'Trader'];

export default function VendorRegistrationPage() {
  const [submitted, setSubmitted] = useState(false);
  const [form, setForm] = useState({
    legalName:       '',
    tradeName:       '',
    vendorType:      'Manufacturer' as VendorType,
    npwp:            '',
    siup:            '',
    nib:             '',
    contactName:     '',
    contactPosition: '',
    contactEmail:    '',
    contactPhone:    '',
  });

  const mutation = useMutation({
    mutationFn: () =>
      vendorRegistrationApi.register({
        companyId:       COMPANY_ID,
        legalName:       form.legalName,
        tradeName:       form.tradeName || undefined,
        vendorType:      form.vendorType,
        npwp:            form.npwp || undefined,
        siup:            form.siup || undefined,
        nib:             form.nib || undefined,
        contactName:     form.contactName,
        contactPosition: form.contactPosition || undefined,
        contactEmail:    form.contactEmail,
        contactPhone:    form.contactPhone || undefined,
      }),
    onSuccess: () => setSubmitted(true),
    onError: (error: unknown) => toast.error(extractApiError(error, 'Registration failed. Please try again.')),
  });

  const field = (label: string, key: keyof typeof form, required = false, type = 'text') => (
    <div>
      <label className="block text-sm font-medium text-slate-700 mb-1">
        {label}{required && <span className="text-red-500 ml-0.5">*</span>}
      </label>
      <input
        type={type}
        value={form[key]}
        onChange={(e) => setForm((f) => ({ ...f, [key]: e.target.value }))}
        required={required}
        className="w-full rounded-lg border border-slate-200 px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500/20 focus:border-blue-400"
      />
    </div>
  );

  if (submitted) {
    return (
      <div className="min-h-screen bg-slate-50 flex items-center justify-center p-4">
        <div className="bg-white rounded-2xl shadow-sm border border-slate-100 p-10 max-w-md w-full text-center">
          <CheckCircle2 className="h-12 w-12 text-emerald-500 mx-auto mb-4" />
          <h2 className="text-xl font-semibold text-slate-900">Registration Submitted</h2>
          <p className="text-sm text-slate-500 mt-2">
            Your vendor registration has been received. Our team will review it and contact you at the email provided.
          </p>
        </div>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-slate-50 py-12 px-4">
      <div className="max-w-2xl mx-auto">
        <div className="text-center mb-8">
          <div className="w-12 h-12 rounded-xl bg-blue-500 flex items-center justify-center text-white text-lg font-bold mx-auto mb-3">
            PH
          </div>
          <h1 className="text-2xl font-bold text-slate-900">Vendor Registration</h1>
          <p className="text-sm text-slate-500 mt-1">Register your company as a vendor on Procure Hub</p>
        </div>

        <form
          onSubmit={(e) => { e.preventDefault(); mutation.mutate(); }}
          className="bg-white rounded-2xl border border-slate-100 shadow-sm p-8 space-y-8"
        >
          <div>
            <h2 className="text-sm font-semibold text-slate-700 mb-4 uppercase tracking-wider">Company Information</h2>
            <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
              {field('Legal Name', 'legalName', true)}
              {field('Trade Name', 'tradeName')}
              <div>
                <label className="block text-sm font-medium text-slate-700 mb-1">
                  Vendor Type<span className="text-red-500 ml-0.5">*</span>
                </label>
                <select
                  value={form.vendorType}
                  onChange={(e) => setForm((f) => ({ ...f, vendorType: e.target.value as VendorType }))}
                  className="w-full rounded-lg border border-slate-200 px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500/20 focus:border-blue-400"
                >
                  {vendorTypes.map((t) => <option key={t}>{t}</option>)}
                </select>
              </div>
              {field('NPWP', 'npwp')}
              {field('SIUP', 'siup')}
              {field('NIB', 'nib')}
            </div>
          </div>

          <div>
            <h2 className="text-sm font-semibold text-slate-700 mb-4 uppercase tracking-wider">Primary Contact</h2>
            <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
              {field('Contact Name', 'contactName', true)}
              {field('Position', 'contactPosition')}
              {field('Email', 'contactEmail', true, 'email')}
              {field('Phone', 'contactPhone')}
            </div>
          </div>

          <Button
            type="submit"
            className="w-full"
            disabled={mutation.isPending}
          >
            {mutation.isPending ? 'Submitting...' : 'Submit Registration'}
          </Button>
        </form>
      </div>
    </div>
  );
}
