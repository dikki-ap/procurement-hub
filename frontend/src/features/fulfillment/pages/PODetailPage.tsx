import { useParams, useNavigate } from 'react-router-dom';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { ArrowLeft, FileText, CheckCircle, Truck, AlertTriangle } from 'lucide-react';
import { toast } from 'sonner';
import { Button } from '@/components/ui/button';
import { fulfillmentApi, type POStatus, type GRNStatus } from '../api/fulfillmentApi';
import { useAuthStore } from '@/stores/authStore';
import { extractApiError } from '@/shared/lib/apiError';
import { fmtDate } from '@/shared/lib/date';

const fmt = (n: number) =>
  new Intl.NumberFormat('id-ID', { style: 'decimal', minimumFractionDigits: 0 }).format(n);

const statusColor: Record<POStatus, string> = {
  Draft:           'bg-gray-100 text-gray-600',
  PendingApproval: 'bg-yellow-50 text-yellow-700',
  Approved:        'bg-blue-50 text-blue-700',
  Issued:          'bg-indigo-50 text-indigo-700',
  Acknowledged:    'bg-purple-50 text-purple-700',
  InDelivery:      'bg-orange-50 text-orange-700',
  Completed:       'bg-emerald-50 text-emerald-700',
  Cancelled:       'bg-red-50 text-red-600',
};

const grnStatusColor: Record<GRNStatus, string> = {
  Draft:       'bg-gray-100 text-gray-600',
  Confirmed:   'bg-emerald-50 text-emerald-700',
  Discrepancy: 'bg-red-50 text-red-600',
};

export default function PODetailPage() {
  const { id }   = useParams<{ id: string }>();
  const navigate = useNavigate();
  const qc       = useQueryClient();
  const { user } = useAuthStore();

  const isPurchasing = user?.roles.some(r => ['purchasing', 'super_admin'].includes(r));

  const { data: po, isLoading } = useQuery({
    queryKey: ['purchase-order', id],
    queryFn:  () => fulfillmentApi.getPOById(id!).then(r => r.data),
    enabled:  !!id,
  });

  const { data: grns = [] } = useQuery({
    queryKey: ['grns', id],
    queryFn:  () => fulfillmentApi.getGRNList(id!).then(r => r.data),
    enabled:  !!id,
  });

  const issueMut = useMutation({
    mutationFn: () => fulfillmentApi.issuePO(id!),
    onSuccess:  () => {
      qc.invalidateQueries({ queryKey: ['purchase-order', id] });
      toast.success('PO issued successfully');
    },
    onError: (error: unknown) => toast.error(extractApiError(error, 'Failed to issue PO')),
  });

  const ackMut = useMutation({
    mutationFn: () => fulfillmentApi.acknowledgePO(id!),
    onSuccess:  () => {
      qc.invalidateQueries({ queryKey: ['purchase-order', id] });
      toast.success('PO acknowledged');
    },
    onError: (error: unknown) => toast.error(extractApiError(error, 'Failed to acknowledge PO')),
  });

  if (isLoading) return <div className="p-6 text-muted-foreground">Loading...</div>;
  if (!po)       return <div className="p-6 text-red-500">PO not found.</div>;

  const currSym = po.currencyCode ?? 'IDR';
  const canCreateGRN = isPurchasing && ['Acknowledged', 'InDelivery'].includes(po.status);

  return (
    <div className="space-y-6 max-w-4xl">
      {/* Header */}
      <div className="flex items-center gap-4">
        <Button variant="ghost" size="sm" onClick={() => navigate(-1)}>
          <ArrowLeft className="h-4 w-4 mr-1" /> Back
        </Button>
        <div className="flex-1">
          <div className="flex items-center gap-3">
            <h1 className="text-2xl font-semibold">{po.poNumber}</h1>
            <span className={`inline-flex px-2.5 py-1 rounded-full text-sm font-medium ${statusColor[po.status]}`}>
              {po.status}
            </span>
          </div>
          <p className="text-sm text-muted-foreground">{po.vendorName}</p>
        </div>
        <div className="flex flex-wrap gap-2">
          {po.status === 'Approved' && isPurchasing && (
            <Button onClick={() => issueMut.mutate()} disabled={issueMut.isPending}>
              <FileText className="h-4 w-4 mr-2" /> Issue PO
            </Button>
          )}
          {po.status === 'Issued' && (
            <Button variant="outline" onClick={() => ackMut.mutate()} disabled={ackMut.isPending}>
              <CheckCircle className="h-4 w-4 mr-2" /> Acknowledge
            </Button>
          )}
          {canCreateGRN && (
            <Button variant="outline" onClick={() => navigate('grns/new')}>
              <Truck className="h-4 w-4 mr-2" /> New GRN
            </Button>
          )}
        </div>
      </div>

      {/* Info Grid */}
      <div className="grid grid-cols-1 sm:grid-cols-2 gap-4 p-4 bg-muted/30 rounded-lg text-sm">
        {[
          { label: 'Total Amount',      value: `${currSym} ${fmt(po.totalAmount)}` },
          { label: 'Currency',          value: po.currencyCode ?? 'IDR' },
          { label: 'Payment Terms',     value: po.paymentTermName ?? '—' },
          { label: 'Delivery Location', value: po.deliveryLocation ?? '—' },
          { label: 'Expected Delivery', value: fmtDate(po.expectedDelivery) },
          { label: 'Actual Delivery',   value: fmtDate(po.actualDelivery) },
          { label: 'Issued At',         value: fmtDate(po.issuedAt) },
          { label: 'Created',           value: fmtDate(po.createdAt) },
        ].map(({ label, value }) => (
          <div key={label}>
            <dt className="text-muted-foreground font-medium">{label}</dt>
            <dd className="mt-0.5 font-semibold">{value}</dd>
          </div>
        ))}
        {po.status === 'Issued' && po.acknowledgementDeadline && (() => {
          const deadline = new Date(po.acknowledgementDeadline);
          const now      = new Date();
          const daysLeft = Math.ceil((deadline.getTime() - now.getTime()) / (1000 * 60 * 60 * 24));
          const isOverdue = daysLeft <= 0;
          return (
            <div className="col-span-2 flex items-center gap-2 p-3 rounded-lg border border-amber-200 bg-amber-50">
              <AlertTriangle className={`h-4 w-4 flex-shrink-0 ${isOverdue ? 'text-red-600' : 'text-amber-600'}`} />
              <div>
                <dt className={`text-xs font-semibold ${isOverdue ? 'text-red-700' : 'text-amber-700'}`}>
                  {isOverdue ? 'Acknowledgement OVERDUE' : 'Acknowledgement Deadline'}
                </dt>
                <dd className={`text-sm font-bold ${isOverdue ? 'text-red-600' : 'text-amber-600'}`}>
                  {fmtDate(po.acknowledgementDeadline)}
                  {!isOverdue && ` — ${daysLeft} day${daysLeft !== 1 ? 's' : ''} remaining`}
                  {isOverdue && ` — ${Math.abs(daysLeft)} day${Math.abs(daysLeft) !== 1 ? 's' : ''} overdue`}
                </dd>
              </div>
            </div>
          );
        })()}
      </div>

      {po.notes && (
        <div className="text-sm">
          <p className="text-muted-foreground font-medium mb-1">Notes</p>
          <p>{po.notes}</p>
        </div>
      )}

      {po.fileUrl && (
        <div>
          <a href={po.fileUrl} download={`${po.poNumber}.pdf`}
            className="inline-flex items-center gap-1 text-sm text-blue-600 hover:underline">
            <FileText className="h-3.5 w-3.5" /> Download PO PDF
          </a>
        </div>
      )}

      {/* Items Table */}
      <div>
        <h2 className="text-base font-semibold mb-3">Items ({po.items.length})</h2>
        <div className="rounded-md border overflow-x-auto">
          <table className="w-full text-sm min-w-[640px]">
            <thead className="bg-muted/50">
              <tr>
                {['#', 'Description', 'Qty', 'Unit', 'Unit Price', 'Total', 'Received'].map(h => (
                  <th key={h} className="px-3 py-2 text-left font-medium text-muted-foreground">{h}</th>
                ))}
              </tr>
            </thead>
            <tbody>
              {po.items.map((item, idx) => (
                <tr key={item.id} className="border-t">
                  <td className="px-3 py-2 text-muted-foreground">{idx + 1}</td>
                  <td className="px-3 py-2 font-medium">{item.description}</td>
                  <td className="px-3 py-2">{item.quantity}</td>
                  <td className="px-3 py-2">{item.uomCode ?? '—'}</td>
                  <td className="px-3 py-2">{currSym} {fmt(item.unitPrice)}</td>
                  <td className="px-3 py-2 font-semibold">{currSym} {fmt(item.totalPrice)}</td>
                  <td className="px-3 py-2">
                    <span className={item.receivedQty >= item.quantity ? 'text-emerald-600 font-semibold' : ''}>
                      {item.receivedQty} / {item.quantity}
                    </span>
                  </td>
                </tr>
              ))}
            </tbody>
            <tfoot className="bg-muted/30 border-t">
              <tr>
                <td colSpan={5} className="px-3 py-2 text-right font-semibold">Total</td>
                <td className="px-3 py-2 font-bold">{currSym} {fmt(po.totalAmount)}</td>
                <td />
              </tr>
            </tfoot>
          </table>
        </div>
      </div>

      {/* GRN List */}
      {grns.length > 0 && (
        <div>
          <h2 className="text-base font-semibold mb-3">Goods Receipts ({grns.length})</h2>
          <div className="rounded-md border overflow-x-auto">
            <table className="w-full text-sm min-w-[500px]">
              <thead className="bg-muted/50">
                <tr>
                  {['GRN Number', 'Status', 'Received At', 'Created'].map(h => (
                    <th key={h} className="px-3 py-2 text-left font-medium text-muted-foreground">{h}</th>
                  ))}
                </tr>
              </thead>
              <tbody>
                {grns.map(grn => (
                  <tr key={grn.id} className="border-t hover:bg-muted/30 cursor-pointer">
                    <td className="px-3 py-2 font-medium">{grn.grnNumber}</td>
                    <td className="px-3 py-2">
                      <span className={`inline-flex px-2 py-0.5 rounded-full text-xs font-medium ${grnStatusColor[grn.status]}`}>
                        {grn.status}
                      </span>
                    </td>
                    <td className="px-3 py-2">
                      {fmtDate(grn.receivedAt)}
                    </td>
                    <td className="px-3 py-2">
                      {fmtDate(grn.createdAt)}
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </div>
      )}
    </div>
  );
}
