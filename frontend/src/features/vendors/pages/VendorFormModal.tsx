import { useMutation, useQueryClient } from '@tanstack/react-query';
import { toast } from 'sonner';
import { Button } from '@/components/ui/button';
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
} from '@/components/ui/dialog';
import { vendorRegistrationApi, type VendorType } from '../api/vendorApi';

const COMPANY_ID = '00000000-0000-0000-0000-000000000001';

const vendorTypes: VendorType[] = ['Manufacturer', 'Distributor', 'Trader'];

const inputCls =
  'w-full rounded-lg border border-slate-200 px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500/20 focus:border-blue-400';

type Props = {
  open: boolean;
  onClose: () => void;
};

export function VendorFormModal({ open, onClose }: Props) {
  const qc = useQueryClient();

  const mutation = useMutation({
    mutationFn: vendorRegistrationApi.register,
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['vendors'] });
      toast.success('Vendor registered successfully', { duration: 3000 });
      onClose();
    },
    onError: () => toast.error('Failed to register vendor'),
  });

  const isPending = mutation.isPending;

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

  return (
    <Dialog open={open} onOpenChange={(v) => { if (!v && !isPending) onClose(); }}>
      <DialogContent className="max-w-2xl max-h-[90vh] overflow-y-auto">
        <DialogHeader>
          <DialogTitle>Add Vendor</DialogTitle>
        </DialogHeader>

        <form onSubmit={handleSubmit} className="space-y-6 mt-2">
          {/* Company Info */}
          <div>
            <p className="text-xs font-semibold text-slate-500 uppercase tracking-wider mb-3">Company Information</p>
            <div className="grid grid-cols-2 gap-4">
              <div>
                <label className="block text-sm font-medium text-slate-700 mb-1">Legal Name <span className="text-red-500">*</span></label>
                <input name="legalName" required className={inputCls} />
              </div>
              <div>
                <label className="block text-sm font-medium text-slate-700 mb-1">Trade Name</label>
                <input name="tradeName" className={inputCls} />
              </div>
              <div>
                <label className="block text-sm font-medium text-slate-700 mb-1">Vendor Type <span className="text-red-500">*</span></label>
                <select name="vendorType" required className={inputCls}>
                  {vendorTypes.map(t => <option key={t} value={t}>{t}</option>)}
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
          <div>
            <p className="text-xs font-semibold text-slate-500 uppercase tracking-wider mb-3">Primary Contact</p>
            <div className="grid grid-cols-2 gap-4">
              <div>
                <label className="block text-sm font-medium text-slate-700 mb-1">Name <span className="text-red-500">*</span></label>
                <input name="contactName" required className={inputCls} />
              </div>
              <div>
                <label className="block text-sm font-medium text-slate-700 mb-1">Position</label>
                <input name="contactPosition" className={inputCls} />
              </div>
              <div>
                <label className="block text-sm font-medium text-slate-700 mb-1">Email <span className="text-red-500">*</span></label>
                <input name="contactEmail" type="email" required className={inputCls} />
              </div>
              <div>
                <label className="block text-sm font-medium text-slate-700 mb-1">Phone</label>
                <input name="contactPhone" className={inputCls} />
              </div>
            </div>
          </div>

          <div className="flex justify-end gap-3 pt-2">
            <Button type="button" variant="outline" onClick={onClose} disabled={isPending}>Cancel</Button>
            <Button type="submit" disabled={isPending}>
              {isPending ? 'Saving...' : 'Register Vendor'}
            </Button>
          </div>
        </form>
      </DialogContent>
    </Dialog>
  );
}
