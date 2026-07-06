import { useParams, useNavigate } from 'react-router-dom';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { ArrowLeft, Lock, Play, BarChart2 } from 'lucide-react';
import { toast } from 'sonner';
import { Button } from '@/components/ui/button';
import { procurementApi, type RFQStatus, type RFQVendorStatus } from '../api/procurementApi';

const StatusBadge = ({ status }: { status: RFQStatus }) => {
  const cfg: Record<RFQStatus, string> = {
    Draft:     'bg-slate-100 text-slate-700',
    Open:      'bg-blue-50 text-blue-700',
    Closed:    'bg-emerald-50 text-emerald-700',
    Cancelled: 'bg-gray-100 text-gray-500',
  };
  return <span className={`inline-flex items-center px-2.5 py-1 rounded-full text-sm font-medium ${cfg[status]}`}>{status}</span>;
};

const VendorStatusBadge = ({ status }: { status: RFQVendorStatus }) => {
  const cfg: Record<RFQVendorStatus, string> = {
    Invited:   'bg-blue-50 text-blue-700',
    Declined:  'bg-red-50 text-red-700',
    Submitted: 'bg-emerald-50 text-emerald-700',
  };
  return <span className={`inline-flex items-center px-2 py-0.5 rounded-full text-xs font-medium ${cfg[status]}`}>{status}</span>;
};

export default function RFQDetailPage() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const qc       = useQueryClient();

  const { data: rfq, isLoading } = useQuery({
    queryKey: ['rfq', id],
    queryFn:  () => procurementApi.getRFQ(id!).then(r => r.data),
    enabled:  !!id,
  });

  const invalidate = () => {
    qc.invalidateQueries({ queryKey: ['rfq', id], exact: true });
    qc.invalidateQueries({ queryKey: ['rfqs'] });
  };

  const openMut = useMutation({
    mutationFn: () => procurementApi.openRFQ(id!),
    onSuccess:  () => { invalidate(); toast.success('RFQ opened for bidding'); },
    onError:    (e: any) => toast.error(e?.response?.data?.message ?? 'Failed to open RFQ'),
  });

  const closeMut = useMutation({
    mutationFn: () => procurementApi.closeRFQ(id!),
    onSuccess:  () => { invalidate(); toast.success('RFQ closed'); },
    onError:    () => toast.error('Failed to close RFQ'),
  });

  if (isLoading) return <div className="p-6 text-muted-foreground">Loading...</div>;
  if (!rfq) return <div className="p-6 text-red-500">RFQ not found.</div>;

  return (
    <div className="space-y-6 max-w-4xl">
      <div className="flex items-center gap-4">
        <Button variant="ghost" size="sm" onClick={() => navigate('/procurement/rfqs')}>
          <ArrowLeft className="h-4 w-4 mr-1" /> Back
        </Button>
        <div className="flex-1">
          <div className="flex items-center gap-3">
            <h1 className="text-2xl font-semibold">{rfq.rfqNumber}</h1>
            <StatusBadge status={rfq.status} />
          </div>
          <p className="text-sm text-muted-foreground">{rfq.title}</p>
        </div>
        <div className="flex gap-2">
          {rfq.status === 'Draft' && (
            <Button onClick={() => openMut.mutate()} disabled={openMut.isPending}>
              <Play className="h-4 w-4 mr-2" /> Open for Bidding
            </Button>
          )}
          {rfq.status === 'Open' && (
            <Button variant="outline" onClick={() => closeMut.mutate()} disabled={closeMut.isPending}>
              <Lock className="h-4 w-4 mr-2" /> Close Bidding
            </Button>
          )}
          {rfq.status === 'Closed' && (
            <Button onClick={() => navigate(`/app/procurement/rfqs/${id}/evaluation`)}>
              <BarChart2 className="h-4 w-4 mr-2" /> Evaluate Bids
            </Button>
          )}
        </div>
      </div>

      {/* Info Grid */}
      <div className="grid grid-cols-2 gap-4 p-4 bg-muted/30 rounded-lg text-sm">
        {[
          { label: 'Bid Deadline',  value: new Date(rfq.bidDeadline).toLocaleString('id-ID') },
          { label: 'Delivery Date', value: rfq.deliveryDate ? new Date(rfq.deliveryDate).toLocaleDateString('id-ID') : '—' },
          { label: 'Items',         value: rfq.itemCount },
          { label: 'Vendors',       value: rfq.vendorCount },
          { label: 'Created',       value: new Date(rfq.createdAt).toLocaleDateString('id-ID') },
          { label: 'Last Updated',  value: new Date(rfq.updatedAt).toLocaleDateString('id-ID') },
        ].map(({ label, value }) => (
          <div key={label}>
            <dt className="text-muted-foreground font-medium">{label}</dt>
            <dd className="mt-0.5 font-semibold">{value}</dd>
          </div>
        ))}
      </div>

      {/* Items */}
      <div>
        <h2 className="text-base font-semibold mb-3">Items</h2>
        <div className="rounded-md border overflow-hidden">
          <table className="w-full text-sm">
            <thead className="bg-muted/50">
              <tr>
                {['#', 'Description', 'Qty', 'Unit'].map(h => (
                  <th key={h} className="px-3 py-2 text-left font-medium text-muted-foreground">{h}</th>
                ))}
              </tr>
            </thead>
            <tbody>
              {rfq.items.map((item, idx) => (
                <tr key={item.id} className="border-t">
                  <td className="px-3 py-2 text-muted-foreground">{idx + 1}</td>
                  <td className="px-3 py-2 font-medium">{item.itemDescription}</td>
                  <td className="px-3 py-2">{item.quantity}</td>
                  <td className="px-3 py-2">{item.unitLabel ?? '—'}</td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      </div>

      {/* Vendors */}
      <div>
        <h2 className="text-base font-semibold mb-3">Invited Vendors</h2>
        {rfq.vendors.length === 0 ? (
          <p className="text-sm text-muted-foreground">No vendors invited yet.</p>
        ) : (
          <div className="rounded-md border overflow-hidden">
            <table className="w-full text-sm">
              <thead className="bg-muted/50">
                <tr>
                  {['Vendor ID', 'Invited At', 'Status', 'Declined Reason'].map(h => (
                    <th key={h} className="px-3 py-2 text-left font-medium text-muted-foreground">{h}</th>
                  ))}
                </tr>
              </thead>
              <tbody>
                {rfq.vendors.map(v => (
                  <tr key={v.id} className="border-t">
                    <td className="px-3 py-2 font-mono text-xs">{v.vendorId.slice(0, 8)}…</td>
                    <td className="px-3 py-2">{new Date(v.invitedAt).toLocaleDateString('id-ID')}</td>
                    <td className="px-3 py-2"><VendorStatusBadge status={v.status} /></td>
                    <td className="px-3 py-2 text-muted-foreground">{v.declinedReason ?? '—'}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </div>

      {rfq.notes && (
        <div className="text-sm">
          <p className="text-muted-foreground font-medium mb-1">Notes</p>
          <p>{rfq.notes}</p>
        </div>
      )}
      {rfq.terms && (
        <div className="text-sm">
          <p className="text-muted-foreground font-medium mb-1">Terms & Conditions</p>
          <p>{rfq.terms}</p>
        </div>
      )}
    </div>
  );
}
