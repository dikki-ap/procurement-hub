import { useState } from 'react';
import { useParams } from 'react-router-dom';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { Receipt, Plus } from 'lucide-react';
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

export default function VendorInvoicesPage() {
  const { vendorId } = useParams<{ vendorId: string }>();
  const qc           = useQueryClient();
  const [showForm, setShowForm] = useState(false);

  const { data: invoices = [], isLoading } = useQuery({
    queryKey: ['vendor-invoices', vendorId],
    queryFn:  () => fulfillmentApi.getVendorInvoices(vendorId!).then(r => r.data),
    enabled:  !!vendorId,
  });

  const { data: pos = [] } = useQuery({
    queryKey: ['vendor-orders', vendorId],
    queryFn:  () => fulfillmentApi.getVendorPOs(vendorId!).then(r => r.data),
    enabled:  !!vendorId,
  });

  const eligiblePOs = pos.filter(p => ['Acknowledged', 'InDelivery', 'Completed'].includes(p.status));

  const submitMut = useMutation({
    mutationFn: (payload: Parameters<typeof fulfillmentApi.submitInvoice>[0]) =>
      fulfillmentApi.submitInvoice(payload),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['vendor-invoices', vendorId] });
      toast.success('Invoice submitted');
      setShowForm(false);
    },
    onError: (error: unknown) => toast.error(extractApiError(error, 'Failed to submit invoice')),
  });

  const handleSubmit = (e: React.FormEvent<HTMLFormElement>) => {
    e.preventDefault();
    const fd = new FormData(e.currentTarget);
    submitMut.mutate({
      poId:      fd.get('poId') as string,
      vendorId:  vendorId!,
      amount:    parseFloat(fd.get('amount') as string),
      taxAmount: parseFloat(fd.get('taxAmount') as string) || 0,
      dueAt:     (fd.get('dueAt') as string) || undefined,
      notes:     (fd.get('notes') as string) || undefined,
    });
  };

  const columns: DataTableColumn<InvoiceListDto>[] = [
    { key: 'invoiceNumber', header: 'Invoice No.', sortable: true },
    { key: 'poNumber',      header: 'PO Number',   sortable: true },
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
      <div className="flex flex-wrap items-center justify-between gap-3">
        <div className="flex items-center gap-2">
          <Receipt className="h-5 w-5 text-muted-foreground flex-shrink-0" />
          <div>
            <h1 className="text-xl sm:text-2xl font-semibold">Invoices</h1>
            <p className="text-sm text-muted-foreground hidden sm:block">Submit and track your invoices</p>
          </div>
        </div>
        <Button size="sm" onClick={() => setShowForm(true)}>
          <Plus className="h-4 w-4 mr-1" /> Submit Invoice
        </Button>
      </div>

      <DataTable
        data={invoices}
        columns={columns}
        isLoading={isLoading}
        searchPlaceholder="Search invoice or PO..."
      />

      {/* Submit Invoice Modal */}
      {showForm && (
        <div
          className="fixed inset-0 bg-black/40 flex items-center justify-center z-50"
          onClick={() => setShowForm(false)}
        >
          <div
            className="bg-white rounded-xl p-6 w-[480px] space-y-4 shadow-xl"
            onClick={e => e.stopPropagation()}
          >
            <h2 className="text-lg font-semibold">Submit Invoice</h2>
            <form onSubmit={handleSubmit} className="space-y-3">
              <div>
                <label className="block text-sm font-medium mb-1">Purchase Order *</label>
                <select name="poId" required className={inputCls}>
                  <option value="">Select PO...</option>
                  {eligiblePOs.map(p => (
                    <option key={p.id} value={p.id}>{p.poNumber}</option>
                  ))}
                </select>
              </div>
              <div className="grid grid-cols-2 gap-3">
                <div>
                  <label className="block text-sm font-medium mb-1">Amount (Rp) *</label>
                  <input name="amount" type="number" required min="1" className={inputCls} placeholder="0" />
                </div>
                <div>
                  <label className="block text-sm font-medium mb-1">Tax Amount (Rp)</label>
                  <input name="taxAmount" type="number" min="0" className={inputCls} placeholder="0" />
                </div>
              </div>
              <div>
                <label className="block text-sm font-medium mb-1">Due Date</label>
                <input name="dueAt" type="date" className={inputCls} />
              </div>
              <div>
                <label className="block text-sm font-medium mb-1">Notes</label>
                <textarea name="notes" rows={2} className={inputCls} placeholder="Optional notes" />
              </div>
              <div className="flex gap-2 justify-end pt-2">
                <Button type="button" variant="outline" onClick={() => setShowForm(false)}>Cancel</Button>
                <Button type="submit" disabled={submitMut.isPending}>
                  {submitMut.isPending ? 'Submitting...' : 'Submit Invoice'}
                </Button>
              </div>
            </form>
          </div>
        </div>
      )}
    </div>
  );
}
