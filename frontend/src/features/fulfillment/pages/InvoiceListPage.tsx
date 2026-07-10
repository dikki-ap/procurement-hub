import { useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { Receipt } from 'lucide-react';
import { toast } from 'sonner';
import { Button } from '@/components/ui/button';
import { DataTable, type DataTableColumn } from '@/shared/components/DataTable';
import { fulfillmentApi, type InvoiceListDto, type InvoiceStatus } from '../api/fulfillmentApi';
import { extractApiError } from '@/shared/lib/apiError';

const statusColor: Record<InvoiceStatus, string> = {
  Submitted:   'bg-blue-50 text-blue-700',
  UnderReview: 'bg-yellow-50 text-yellow-700',
  Approved:    'bg-emerald-50 text-emerald-700',
  Paid:        'bg-purple-50 text-purple-700',
  Rejected:    'bg-red-50 text-red-600',
};

const fmt = (n: number) =>
  new Intl.NumberFormat('id-ID', { style: 'decimal', minimumFractionDigits: 0 }).format(n);

const inputCls =
  'w-full rounded-lg border border-slate-200 px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500/20 focus:border-blue-400';

export default function InvoiceListPage() {
  const qc = useQueryClient();
  const [reviewingId, setReviewingId]   = useState<string | null>(null);
  const [paymentId, setPaymentId]       = useState<string | null>(null);
  const [payRef, setPayRef]             = useState('');
  const [rejectReason, setRejectReason] = useState('');

  const { data: invoices = [], isLoading } = useQuery({
    queryKey: ['invoices'],
    queryFn:  fulfillmentApi.getInvoiceList,
  });

  const reviewMut = useMutation({
    mutationFn: ({ id, approve, reason }: { id: string; approve: boolean; reason?: string }) =>
      fulfillmentApi.reviewInvoice(id, approve, reason),
    onSuccess: (_, { approve }) => {
      qc.invalidateQueries({ queryKey: ['invoices'] });
      toast.success(approve ? 'Invoice approved' : 'Invoice rejected');
      setReviewingId(null);
      setRejectReason('');
    },
    onError: (error: unknown) => toast.error(extractApiError(error, 'Review failed')),
  });

  const payMut = useMutation({
    mutationFn: ({ id, ref }: { id: string; ref: string }) =>
      fulfillmentApi.confirmPayment(id, ref),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['invoices'] });
      toast.success('Payment confirmed');
      setPaymentId(null);
      setPayRef('');
    },
    onError: (error: unknown) => toast.error(extractApiError(error, 'Payment confirmation failed')),
  });

  const columns: DataTableColumn<InvoiceListDto>[] = [
    { key: 'invoiceNumber', header: 'Invoice No.', sortable: true },
    { key: 'poNumber',      header: 'PO Number',   sortable: true },
    { key: 'vendorName',    header: 'Vendor',       sortable: true },
    {
      key: 'status',
      header: 'Status',
      render: (row) => (
        <span className={`inline-flex px-2 py-0.5 rounded-full text-xs font-medium ${statusColor[row.status]}`}>
          {row.status}
        </span>
      ),
    },
    {
      key: 'totalAmount',
      header: 'Total (Rp)',
      render: (row) => fmt(row.totalAmount),
    },
    {
      key: 'dueAt',
      header: 'Due Date',
      render: (row) =>
        row.dueAt ? new Date(row.dueAt).toLocaleDateString('id-ID') : '—',
    },
    {
      key: 'submittedAt',
      header: 'Submitted',
      render: (row) => new Date(row.submittedAt).toLocaleDateString('id-ID'),
    },
  ];

  return (
    <div className="space-y-4">
      <div className="flex flex-wrap items-center justify-between gap-3 mb-2">
        <div className="flex items-center gap-2">
          <Receipt className="h-5 w-5 text-muted-foreground flex-shrink-0" />
          <div>
            <h1 className="text-xl sm:text-2xl font-semibold">Invoices</h1>
            <p className="text-sm text-muted-foreground hidden sm:block">Review and process vendor invoices</p>
          </div>
        </div>
      </div>

      <DataTable
        data={invoices}
        columns={columns}
        isLoading={isLoading}
        searchPlaceholder="Search invoice or vendor..."
        rowActions={(row) => (
          <div className="flex gap-1">
            {row.status === 'Submitted' && (
              <Button size="sm" variant="outline" onClick={() => setReviewingId(row.id)}>
                Review
              </Button>
            )}
            {row.status === 'Approved' && (
              <Button size="sm" variant="outline" onClick={() => setPaymentId(row.id)}>
                Pay
              </Button>
            )}
          </div>
        )}
      />

      {/* Review Modal */}
      {reviewingId && (
        <div
          className="fixed inset-0 bg-black/40 flex items-center justify-center z-50"
          onClick={() => setReviewingId(null)}
        >
          <div
            className="bg-white rounded-xl p-6 w-96 space-y-4 shadow-xl"
            onClick={e => e.stopPropagation()}
          >
            <h2 className="text-lg font-semibold">Review Invoice</h2>
            <div>
              <label className="block text-sm font-medium mb-1">
                Rejection Reason (required if rejecting)
              </label>
              <textarea rows={3} className={inputCls}
                value={rejectReason}
                onChange={e => setRejectReason(e.target.value)}
                placeholder="Enter reason if rejecting..." />
            </div>
            <div className="flex gap-2 justify-end">
              <Button variant="outline" onClick={() => setReviewingId(null)}>Cancel</Button>
              <Button
                variant="outline"
                className="text-red-600 border-red-200"
                onClick={() => reviewMut.mutate({ id: reviewingId, approve: false, reason: rejectReason || undefined })}
                disabled={reviewMut.isPending}
              >
                Reject
              </Button>
              <Button
                onClick={() => reviewMut.mutate({ id: reviewingId, approve: true })}
                disabled={reviewMut.isPending}
              >
                Approve
              </Button>
            </div>
          </div>
        </div>
      )}

      {/* Payment Modal */}
      {paymentId && (
        <div
          className="fixed inset-0 bg-black/40 flex items-center justify-center z-50"
          onClick={() => setPaymentId(null)}
        >
          <div
            className="bg-white rounded-xl p-6 w-96 space-y-4 shadow-xl"
            onClick={e => e.stopPropagation()}
          >
            <h2 className="text-lg font-semibold">Confirm Payment</h2>
            <div>
              <label className="block text-sm font-medium mb-1">Payment Reference *</label>
              <input className={inputCls}
                value={payRef}
                onChange={e => setPayRef(e.target.value)}
                placeholder="e.g. TRF-20250706-001" />
            </div>
            <div className="flex gap-2 justify-end">
              <Button variant="outline" onClick={() => setPaymentId(null)}>Cancel</Button>
              <Button
                onClick={() => payMut.mutate({ id: paymentId, ref: payRef })}
                disabled={payMut.isPending || !payRef.trim()}
              >
                Confirm Payment
              </Button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}
