import { useState } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import {
  ArrowLeft, Receipt, CheckCircle2, XCircle, AlertTriangle,
  Clock, Building2, CreditCard, AlertCircle,
} from 'lucide-react';
import { toast } from 'sonner';
import { Button } from '@/components/ui/button';
import {
  Dialog, DialogContent, DialogHeader, DialogTitle,
} from '@/components/ui/dialog';
import { fmtDate, fmtDateTime } from '@/shared/lib/date';
import { fulfillmentApi, type InvoiceStatus } from '../api/fulfillmentApi';
import { extractApiError } from '@/shared/lib/apiError';
import { useAuthStore } from '@/stores/authStore';

const inputCls =
  'w-full rounded-md border border-input bg-transparent px-3 py-2 text-sm shadow-sm transition-colors placeholder:text-muted-foreground focus-visible:outline-none focus-visible:ring-1 focus-visible:ring-ring disabled:cursor-not-allowed disabled:opacity-50';

const STATUS_CFG: Record<InvoiceStatus, { label: string; cls: string }> = {
  Submitted:   { label: 'Submitted',    cls: 'bg-blue-50 text-blue-700' },
  UnderReview: { label: 'Under Review', cls: 'bg-yellow-50 text-yellow-700' },
  Approved:    { label: 'Approved',     cls: 'bg-emerald-50 text-emerald-700' },
  Paid:        { label: 'Paid',         cls: 'bg-purple-50 text-purple-700' },
  Rejected:    { label: 'Rejected',     cls: 'bg-red-50 text-red-600' },
};

function InfoRow({ label, value }: { label: string; value: React.ReactNode }) {
  return (
    <div className="flex flex-col gap-0.5">
      <span className="text-xs text-slate-500 uppercase tracking-wide">{label}</span>
      <span className="text-sm font-medium text-slate-800">{value ?? '—'}</span>
    </div>
  );
}

function MatchChip({ ok, label }: { ok: boolean; label: string }) {
  return (
    <span className={`inline-flex items-center gap-1.5 px-3 py-1 rounded-full text-xs font-medium ${
      ok ? 'bg-emerald-50 text-emerald-700' : 'bg-red-50 text-red-600'
    }`}>
      {ok
        ? <CheckCircle2 className="h-3.5 w-3.5" />
        : <XCircle className="h-3.5 w-3.5" />}
      {label}
    </span>
  );
}

const fmt = (n: number) =>
  new Intl.NumberFormat('id-ID', { minimumFractionDigits: 0 }).format(n);

export default function InvoiceDetailPage() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const qc = useQueryClient();

  const isFinance = useAuthStore(s =>
    s.user?.roles?.some(r => ['finance', 'super_admin'].includes(r)) ?? false);

  const [showReject, setShowReject]       = useState(false);
  const [rejectReason, setRejectReason]   = useState('');
  const [showPayment, setShowPayment]     = useState(false);
  const [payRef, setPayRef]               = useState('');

  const { data: invoice, isLoading } = useQuery({
    queryKey: ['invoice', id],
    queryFn: () => fulfillmentApi.getInvoiceById(id!),
    enabled: !!id,
  });

  const invalidate = () => qc.invalidateQueries({ queryKey: ['invoice', id] });

  const approveMut = useMutation({
    mutationFn: () => fulfillmentApi.reviewInvoice(id!, true),
    onSuccess: () => { toast.success('Invoice approved'); invalidate(); },
    onError: (e: unknown) => toast.error(extractApiError(e, 'Approval failed')),
  });

  const rejectMut = useMutation({
    mutationFn: () => fulfillmentApi.reviewInvoice(id!, false, rejectReason),
    onSuccess: () => {
      toast.success('Invoice rejected');
      invalidate();
      setShowReject(false);
      setRejectReason('');
    },
    onError: (e: unknown) => toast.error(extractApiError(e, 'Rejection failed')),
  });

  const payMut = useMutation({
    mutationFn: () => fulfillmentApi.confirmPayment(id!, payRef),
    onSuccess: () => {
      toast.success('Payment confirmed');
      invalidate();
      setShowPayment(false);
      setPayRef('');
    },
    onError: (e: unknown) => toast.error(extractApiError(e, 'Payment confirmation failed')),
  });

  if (isLoading || !invoice) {
    return (
      <div className="animate-pulse space-y-4">
        <div className="h-8 bg-slate-100 rounded w-1/3" />
        <div className="h-64 bg-slate-100 rounded" />
      </div>
    );
  }

  const statusCfg     = STATUS_CFG[invoice.status];
  const canApprove    = isFinance && invoice.status === 'UnderReview';
  const canReject     = isFinance && (invoice.status === 'Submitted' || invoice.status === 'UnderReview');
  const canPay        = isFinance && invoice.status === 'Approved';
  const allMatched    = invoice.poMatched && invoice.grnMatched && invoice.amountMatched;
  const approveBlocked = canApprove && !allMatched;

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex flex-wrap items-center justify-between gap-3">
        <div className="flex items-center gap-3">
          <button
            onClick={() => navigate('/app/fulfillment/invoices')}
            className="flex items-center gap-1 text-sm text-slate-500 hover:text-slate-800 transition-colors"
          >
            <ArrowLeft className="h-4 w-4" /> Invoices
          </button>
          <span className="text-slate-300">/</span>
          <span className="text-sm font-mono font-semibold text-slate-700">{invoice.invoiceNumber}</span>
          <span className={`inline-flex items-center px-2.5 py-1 rounded-full text-xs font-medium ${statusCfg.cls}`}>
            {statusCfg.label}
          </span>
        </div>

        {isFinance && (
          <div className="flex items-center gap-2">
            {canApprove && (
              <Button
                size="sm"
                onClick={() => approveMut.mutate()}
                disabled={approveMut.isPending || approveBlocked}
                title={approveBlocked ? 'Resolve matching discrepancies before approving' : undefined}
                className="bg-emerald-600 hover:bg-emerald-700 disabled:opacity-50"
              >
                <CheckCircle2 className="h-3.5 w-3.5 mr-1" />
                {approveMut.isPending ? 'Approving…' : 'Approve'}
              </Button>
            )}
            {canReject && (
              <Button size="sm" variant="destructive" onClick={() => setShowReject(true)}>
                <XCircle className="h-3.5 w-3.5 mr-1" /> Reject
              </Button>
            )}
            {canPay && (
              <Button size="sm" onClick={() => setShowPayment(true)}>
                <CreditCard className="h-3.5 w-3.5 mr-1" /> Confirm Payment
              </Button>
            )}
          </div>
        )}
      </div>

      {/* 3-Way Matching Panel */}
      <div className="bg-white rounded-xl border border-slate-100 p-5">
        <h2 className="text-sm font-semibold text-slate-700 mb-3">3-Way Matching</h2>
        <div className="flex flex-wrap gap-2 mb-3">
          <MatchChip ok={invoice.poMatched}     label="PO Matched" />
          <MatchChip ok={invoice.grnMatched}    label="GRN Matched" />
          <MatchChip ok={invoice.amountMatched} label="Amount Matched" />
        </div>
        {invoice.matchingDiscrepancies.length > 0 && (
          <div className="mt-3 p-3 rounded-lg bg-red-50 border border-red-100 space-y-1">
            <p className="text-xs font-semibold text-red-600 flex items-center gap-1.5">
              <AlertTriangle className="h-3.5 w-3.5" /> Discrepancies
            </p>
            {invoice.matchingDiscrepancies.map((d, i) => (
              <p key={i} className="text-xs text-red-600 ml-5">• {d}</p>
            ))}
          </div>
        )}
        {approveBlocked && (
          <p className="mt-3 flex items-center gap-1.5 text-xs text-amber-600">
            <AlertCircle className="h-3.5 w-3.5" />
            Resolve all matching issues before approving.
          </p>
        )}
      </div>

      {/* Invoice Info */}
      <div className="bg-white rounded-xl border border-slate-100 p-6">
        <h2 className="text-lg font-semibold text-slate-900 mb-5">{invoice.invoiceNumber}</h2>
        <div className="grid grid-cols-2 sm:grid-cols-3 gap-5">
          <InfoRow label="Vendor" value={
            <div className="flex items-center gap-1.5">
              <Building2 className="h-3.5 w-3.5 text-slate-400" />
              {invoice.vendorName || '—'}
            </div>
          } />
          <InfoRow label="PO Number" value={invoice.poNumber || '—'} />
          <InfoRow label="Due Date"  value={fmtDate(invoice.dueAt)} />
          <InfoRow label="Amount (DPP)" value={fmt(invoice.amount)} />
          <InfoRow label="PPN (11%)"    value={fmt(invoice.taxAmount)} />
          <InfoRow label="Total"        value={<span className="font-semibold">{fmt(invoice.totalAmount)}</span>} />
          {invoice.withholdingTax > 0 && (
            <>
              <InfoRow label="PPh (Withholding)" value={<span className="text-red-600">- {fmt(invoice.withholdingTax)}</span>} />
              <InfoRow label="Net Payable"        value={<span className="font-semibold text-emerald-700">{fmt(invoice.netPayable)}</span>} />
            </>
          )}
          {invoice.status === 'Paid' && (
            <>
              <InfoRow label="Paid At"          value={fmtDate(invoice.paidAt)} />
              <InfoRow label="Payment Reference" value={invoice.paymentReference} />
            </>
          )}
        </div>

        {invoice.notes && (
          <div className="mt-5 pt-5 border-t border-slate-100">
            <p className="text-xs text-slate-500 uppercase tracking-wide mb-1">Notes</p>
            <p className="text-sm text-slate-700">{invoice.notes}</p>
          </div>
        )}

        {invoice.rejectionReason && (
          <div className="mt-5 flex items-start gap-2 p-3 rounded-lg bg-red-50 border border-red-100">
            <AlertTriangle className="h-4 w-4 text-red-500 flex-shrink-0 mt-0.5" />
            <div>
              <p className="text-xs font-semibold text-red-600">Rejection Reason</p>
              <p className="text-sm text-red-700">{invoice.rejectionReason}</p>
            </div>
          </div>
        )}
      </div>

      {/* Audit */}
      <div className="bg-white rounded-xl border border-slate-100 p-4">
        <div className="flex flex-wrap gap-6 text-xs text-slate-500">
          <div className="flex items-center gap-1">
            <Clock className="h-3.5 w-3.5" />
            Submitted {fmtDateTime(invoice.submittedAt)}
          </div>
          {invoice.reviewedAt && (
            <div className="flex items-center gap-1">
              <Clock className="h-3.5 w-3.5" />
              Reviewed {fmtDateTime(invoice.reviewedAt)}
            </div>
          )}
        </div>
      </div>

      {/* Reject Modal */}
      <Dialog open={showReject} onOpenChange={(v) => { if (!v) setShowReject(false); }}>
        <DialogContent className="max-w-sm">
          <DialogHeader><DialogTitle>Reject Invoice</DialogTitle></DialogHeader>
          <div className="mt-2 space-y-3">
            <p className="text-sm text-slate-600">
              Reject <strong>{invoice.invoiceNumber}</strong>? This will notify the vendor.
            </p>
            <div>
              <label className="block text-sm font-medium mb-1">Reason <span className="text-red-500">*</span></label>
              <textarea
                rows={3} className={inputCls} value={rejectReason}
                onChange={(e) => setRejectReason(e.target.value)}
                placeholder="e.g. Amount does not match PO"
              />
            </div>
            <div className="flex justify-end gap-2 pt-1">
              <Button variant="outline" onClick={() => setShowReject(false)}>Cancel</Button>
              <Button
                variant="destructive"
                disabled={!rejectReason.trim() || rejectMut.isPending}
                onClick={() => rejectMut.mutate()}
              >
                {rejectMut.isPending ? 'Rejecting…' : 'Reject'}
              </Button>
            </div>
          </div>
        </DialogContent>
      </Dialog>

      {/* Payment Modal */}
      <Dialog open={showPayment} onOpenChange={(v) => { if (!v) setShowPayment(false); }}>
        <DialogContent className="max-w-sm">
          <DialogHeader><DialogTitle>Confirm Payment</DialogTitle></DialogHeader>
          <div className="mt-2 space-y-3">
            <p className="text-sm text-slate-600">
              Confirm payment for <strong>{invoice.invoiceNumber}</strong> ({fmt(invoice.totalAmount)}).
            </p>
            <div>
              <label className="block text-sm font-medium mb-1">Payment Reference <span className="text-red-500">*</span></label>
              <input
                className={inputCls} value={payRef}
                onChange={(e) => setPayRef(e.target.value)}
                placeholder="e.g. TRF-20260722-001"
              />
            </div>
            <div className="flex justify-end gap-2 pt-1">
              <Button variant="outline" onClick={() => setShowPayment(false)}>Cancel</Button>
              <Button
                disabled={!payRef.trim() || payMut.isPending}
                onClick={() => payMut.mutate()}
              >
                {payMut.isPending ? 'Confirming…' : 'Confirm Payment'}
              </Button>
            </div>
          </div>
        </DialogContent>
      </Dialog>
    </div>
  );
}
