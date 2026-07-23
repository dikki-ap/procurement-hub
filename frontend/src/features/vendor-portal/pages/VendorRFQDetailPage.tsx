import { useState } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { ArrowLeft, Clock, Send, Undo2 } from 'lucide-react';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { procurementApi, type SubmitQuotationItemRequest } from '@/features/procurement/api/procurementApi';
import { useAuthStore } from '@/stores/authStore';
import { fmtDate, fmtDateTime } from '@/shared/lib/date';

export default function VendorRFQDetailPage() {
  const { id, vendorId } = useParams<{ id: string; vendorId: string }>();
  const navigate          = useNavigate();
  const queryClient       = useQueryClient();
  const { user }          = useAuthStore();

  const { data: rfq, isLoading } = useQuery({
    queryKey: ['vendor-rfq', id],
    queryFn:  () => procurementApi.getRFQ(id!).then(r => r.data),
    enabled:  !!id,
  });

  const { data: myQuotations = [] } = useQuery({
    queryKey: ['my-quotations', vendorId],
    queryFn:  () => procurementApi.getMyQuotations(vendorId!).then(r => r.data),
    enabled:  !!vendorId,
  });

  const existingQuotation = myQuotations.find(q => q.rfqId === id);
  const isSubmitted       = existingQuotation?.status === 'Submitted';

  const [prices, setPrices] = useState<Record<string, string>>({});
  const [notes,  setNotes]  = useState('');

  const submitMutation = useMutation({
    mutationFn: () => procurementApi.submitQuotation({
      rfqId:    id!,
      vendorId: vendorId!,
      notes:    notes || undefined,
      items: rfq!.items.map(item => ({
        rfqItemId: item.id,
        unitPrice: parseFloat(prices[item.id] ?? '0'),
      } satisfies SubmitQuotationItemRequest)),
    }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['my-quotations', vendorId] });
      queryClient.invalidateQueries({ queryKey: ['vendor-rfq', id] });
    },
  });

  const withdrawMutation = useMutation({
    mutationFn: () => procurementApi.withdrawQuotation(existingQuotation!.id),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['my-quotations', vendorId] }),
  });

  if (isLoading) return <div className="p-6 text-muted-foreground">Loading...</div>;
  if (!rfq)      return <div className="p-6 text-red-500">RFQ not found.</div>;

  const hoursLeft = (new Date(rfq.bidDeadline).getTime() - Date.now()) / 3_600_000;
  const isUrgent  = hoursLeft > 0 && hoursLeft <= 24;
  const isOpen    = rfq.status === 'Open';

  const allPricesFilled = rfq.items.every(item => {
    const v = parseFloat(prices[item.id] ?? '');
    return !isNaN(v) && v > 0;
  });

  return (
    <div className="space-y-6 max-w-3xl">
      <div className="flex items-center gap-4">
        <Button variant="ghost" size="sm" onClick={() => navigate(`/app/vendor-portal/${vendorId}/bids`)}>
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
            ? `Bid deadline in less than 24 hours: ${fmtDateTime(rfq.bidDeadline)}`
            : `Bid deadline: ${fmtDateTime(rfq.bidDeadline)}`}
        </div>
      )}

      {/* Quotation status banner */}
      {existingQuotation && (
        <div className={`flex items-center justify-between p-3 rounded-lg border text-sm
          ${isSubmitted ? 'bg-green-50 border-green-200 text-green-800' : 'bg-gray-50 border-gray-200 text-gray-600'}`}>
          <span>Your quotation status: <strong>{existingQuotation.status}</strong></span>
          {isSubmitted && isOpen && (
            <Button size="sm" variant="outline" onClick={() => withdrawMutation.mutate()}
              disabled={withdrawMutation.isPending}>
              <Undo2 className="h-3.5 w-3.5 mr-1" />
              {withdrawMutation.isPending ? 'Withdrawing…' : 'Withdraw'}
            </Button>
          )}
        </div>
      )}

      {/* Info grid */}
      <div className="grid grid-cols-1 sm:grid-cols-2 gap-4 p-4 bg-muted/30 rounded-lg text-sm">
        {[
          { label: 'Status',        value: rfq.status },
          { label: 'Items',         value: rfq.itemCount },
          { label: 'Delivery Date', value: fmtDate(rfq.deliveryDate) },
          { label: 'Published',     value: fmtDate(rfq.createdAt) },
        ].map(({ label, value }) => (
          <div key={label}>
            <dt className="text-muted-foreground font-medium">{label}</dt>
            <dd className="mt-0.5 font-semibold">{value}</dd>
          </div>
        ))}
      </div>

      {/* Items to quote */}
      {isOpen && !isSubmitted ? (
        <div>
          <h2 className="text-base font-semibold mb-3">Your Quotation</h2>
          <div className="rounded-md border overflow-x-auto">
            <table className="w-full text-sm min-w-[500px]">
              <thead className="bg-muted/50">
                <tr>
                  {['#', 'Description', 'Qty', 'Unit', 'Unit Price (IDR)'].map(h => (
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
                    <td className="px-3 py-2 w-40">
                      <Input
                        type="number"
                        min={0}
                        step="0.01"
                        placeholder="0"
                        value={prices[item.id] ?? ''}
                        onChange={e => setPrices(p => ({ ...p, [item.id]: e.target.value }))}
                        className="h-8 text-sm"
                      />
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
          <div className="mt-3 flex justify-end">
            <Button
              onClick={() => submitMutation.mutate()}
              disabled={!allPricesFilled || submitMutation.isPending}
            >
              <Send className="h-4 w-4 mr-2" />
              {submitMutation.isPending ? 'Submitting…' : 'Submit Quotation'}
            </Button>
          </div>
          {submitMutation.isError && (
            <p className="text-sm text-red-600 mt-2">Failed to submit. Please check your prices and try again.</p>
          )}
        </div>
      ) : (
        <div>
          <h2 className="text-base font-semibold mb-3">Items</h2>
          <div className="rounded-md border overflow-x-auto">
            <table className="w-full text-sm min-w-[400px]">
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
      )}

      {rfq.terms && (
        <div className="text-sm">
          <p className="text-muted-foreground font-medium mb-1">Terms & Conditions</p>
          <p className="whitespace-pre-wrap">{rfq.terms}</p>
        </div>
      )}
    </div>
  );
}
