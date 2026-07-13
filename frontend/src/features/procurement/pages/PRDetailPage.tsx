import { useParams, useNavigate } from 'react-router-dom';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { ArrowLeft, CheckCircle, XCircle } from 'lucide-react';
import { toast } from 'sonner';
import { Button } from '@/components/ui/button';
import { procurementApi, type PRStatus } from '../api/procurementApi';
import { extractApiError } from '@/shared/lib/apiError';
import { fmtDate } from '@/shared/lib/date';

const fmt = new Intl.NumberFormat('id-ID', { style: 'currency', currency: 'IDR', maximumFractionDigits: 0 });

const StatusBadge = ({ status }: { status: PRStatus }) => {
  const cfg: Record<PRStatus, string> = {
    Draft:     'bg-slate-100 text-slate-700',
    Submitted: 'bg-blue-50 text-blue-700',
    Approved:  'bg-emerald-50 text-emerald-700',
    Rejected:  'bg-red-50 text-red-700',
    Cancelled: 'bg-gray-100 text-gray-500',
  };
  return (
    <span className={`inline-flex items-center px-2.5 py-1 rounded-full text-sm font-medium ${cfg[status]}`}>
      {status}
    </span>
  );
};

export default function PRDetailPage() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const qc       = useQueryClient();

  const { data: pr, isLoading } = useQuery({
    queryKey: ['pr', id],
    queryFn:  () => procurementApi.getPR(id!).then(r => r.data),
    enabled:  !!id,
  });

  const submitMut = useMutation({
    mutationFn: () => procurementApi.submitPR(id!),
    onSuccess:  () => { qc.invalidateQueries({ queryKey: ['pr', id], exact: true }); qc.invalidateQueries({ queryKey: ['prs'] }); toast.success('PR submitted'); },
    onError:    (error: unknown) => toast.error(extractApiError(error, 'Failed to submit PR')),
  });

  const cancelMut = useMutation({
    mutationFn: () => procurementApi.cancelPR(id!),
    onSuccess:  () => { qc.invalidateQueries({ queryKey: ['pr', id], exact: true }); qc.invalidateQueries({ queryKey: ['prs'] }); toast.success('PR cancelled'); },
    onError:    (error: unknown) => toast.error(extractApiError(error, 'Failed to cancel PR')),
  });

  if (isLoading) return <div className="p-6 text-muted-foreground">Loading...</div>;
  if (!pr) return <div className="p-6 text-red-500">PR not found.</div>;

  return (
    <div className="space-y-6 max-w-4xl">
      <div className="flex items-center gap-4">
        <Button variant="ghost" size="sm" onClick={() => navigate('/procurement/prs')}>
          <ArrowLeft className="h-4 w-4 mr-1" /> Back
        </Button>
        <div className="flex-1">
          <div className="flex items-center gap-3">
            <h1 className="text-2xl font-semibold">{pr.prNumber}</h1>
            <StatusBadge status={pr.status} />
          </div>
          <p className="text-sm text-muted-foreground">{pr.title}</p>
        </div>
        <div className="flex gap-2">
          {pr.status === 'Draft' && (
            <Button onClick={() => submitMut.mutate()} disabled={submitMut.isPending}>
              <CheckCircle className="h-4 w-4 mr-2" /> Submit PR
            </Button>
          )}
          {(pr.status === 'Draft' || pr.status === 'Submitted') && (
            <Button variant="outline" className="text-red-600 border-red-200"
              onClick={() => cancelMut.mutate()} disabled={cancelMut.isPending}>
              <XCircle className="h-4 w-4 mr-2" /> Cancel
            </Button>
          )}
        </div>
      </div>

      {/* Info Grid */}
      <div className="grid grid-cols-2 gap-4 p-4 bg-muted/30 rounded-lg text-sm">
        {[
          { label: 'Department',       value: pr.department },
          { label: 'Required Date',    value: fmtDate(pr.requiredDate) },
          { label: 'Delivery Location', value: pr.deliveryLocation ?? '—' },
          { label: 'Total Est. Value', value: fmt.format(pr.totalEstimatedValue) },
          { label: 'Created',          value: fmtDate(pr.createdAt) },
          { label: 'Last Updated',     value: fmtDate(pr.updatedAt) },
        ].map(({ label, value }) => (
          <div key={label}>
            <dt className="text-muted-foreground font-medium">{label}</dt>
            <dd className="mt-0.5 font-semibold">{value}</dd>
          </div>
        ))}
      </div>

      {pr.description && (
        <div className="text-sm">
          <p className="text-muted-foreground font-medium mb-1">Description</p>
          <p>{pr.description}</p>
        </div>
      )}

      {/* Items Table */}
      <div>
        <h2 className="text-base font-semibold mb-3">Items ({pr.items.length})</h2>
        <div className="rounded-md border overflow-hidden">
          <table className="w-full text-sm">
            <thead className="bg-muted/50">
              <tr>
                {['#', 'Description', 'Qty', 'Unit', 'Unit Price', 'Line Total', 'Notes'].map(h => (
                  <th key={h} className="px-3 py-2 text-left font-medium text-muted-foreground">{h}</th>
                ))}
              </tr>
            </thead>
            <tbody>
              {pr.items.map((item, idx) => (
                <tr key={item.id} className="border-t">
                  <td className="px-3 py-2 text-muted-foreground">{idx + 1}</td>
                  <td className="px-3 py-2 font-medium">{item.itemDescription}</td>
                  <td className="px-3 py-2">{item.quantity}</td>
                  <td className="px-3 py-2">{item.unitLabel ?? '—'}</td>
                  <td className="px-3 py-2">{fmt.format(item.estimatedUnitPrice)}</td>
                  <td className="px-3 py-2 font-semibold">{fmt.format(item.lineTotal)}</td>
                  <td className="px-3 py-2 text-muted-foreground">{item.notes ?? '—'}</td>
                </tr>
              ))}
            </tbody>
            <tfoot className="bg-muted/30 border-t">
              <tr>
                <td colSpan={5} className="px-3 py-2 text-right font-semibold">Total</td>
                <td className="px-3 py-2 font-bold">{fmt.format(pr.totalEstimatedValue)}</td>
                <td />
              </tr>
            </tfoot>
          </table>
        </div>
      </div>
    </div>
  );
}
