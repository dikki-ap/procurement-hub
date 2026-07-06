import { useParams, useNavigate } from 'react-router-dom';
import { useQuery } from '@tanstack/react-query';
import { ArrowLeft, Clock } from 'lucide-react';
import { Button } from '@/components/ui/button';
import { procurementApi } from '@/features/procurement/api/procurementApi';

export default function VendorRFQDetailPage() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();

  const { data: rfq, isLoading } = useQuery({
    queryKey: ['vendor-rfq', id],
    queryFn:  () => procurementApi.getRFQ(id!).then(r => r.data),
    enabled:  !!id,
  });

  if (isLoading) return <div className="p-6 text-muted-foreground">Loading...</div>;
  if (!rfq) return <div className="p-6 text-red-500">RFQ not found.</div>;

  const hoursLeft = (new Date(rfq.bidDeadline).getTime() - Date.now()) / 3_600_000;
  const isUrgent  = hoursLeft > 0 && hoursLeft <= 24;
  const isOpen    = rfq.status === 'Open';

  return (
    <div className="space-y-6 max-w-3xl">
      <div className="flex items-center gap-4">
        <Button variant="ghost" size="sm" onClick={() => navigate('/vendor/bids')}>
          <ArrowLeft className="h-4 w-4 mr-1" /> Back to Bids
        </Button>
        <div className="flex-1">
          <h1 className="text-2xl font-semibold">{rfq.rfqNumber}</h1>
          <p className="text-sm text-muted-foreground">{rfq.title}</p>
        </div>
      </div>

      {/* Deadline banner */}
      {isOpen && (
        <div className={`flex items-center gap-2 p-3 rounded-lg text-sm font-medium
          ${isUrgent ? 'bg-amber-50 text-amber-800 border border-amber-200' : 'bg-blue-50 text-blue-800 border border-blue-200'}`}>
          <Clock className="h-4 w-4" />
          {isUrgent
            ? `Bid deadline in less than 24 hours: ${new Date(rfq.bidDeadline).toLocaleString('id-ID')}`
            : `Bid deadline: ${new Date(rfq.bidDeadline).toLocaleString('id-ID')}`}
        </div>
      )}

      {/* Info */}
      <div className="grid grid-cols-2 gap-4 p-4 bg-muted/30 rounded-lg text-sm">
        {[
          { label: 'Status',       value: rfq.status },
          { label: 'Items',        value: rfq.itemCount },
          { label: 'Delivery Date', value: rfq.deliveryDate ? new Date(rfq.deliveryDate).toLocaleDateString('id-ID') : '—' },
          { label: 'Published',    value: new Date(rfq.createdAt).toLocaleDateString('id-ID') },
        ].map(({ label, value }) => (
          <div key={label}>
            <dt className="text-muted-foreground font-medium">{label}</dt>
            <dd className="mt-0.5 font-semibold">{value}</dd>
          </div>
        ))}
      </div>

      {/* Items */}
      <div>
        <h2 className="text-base font-semibold mb-3">Items to Quote</h2>
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

      {rfq.terms && (
        <div className="text-sm">
          <p className="text-muted-foreground font-medium mb-1">Terms & Conditions</p>
          <p className="whitespace-pre-wrap">{rfq.terms}</p>
        </div>
      )}

      {isOpen && (
        <div className="p-4 bg-muted/30 rounded-lg text-sm text-muted-foreground">
          Quotation submission will be available in Phase 5 (Bidding & Evaluation).
        </div>
      )}
    </div>
  );
}
