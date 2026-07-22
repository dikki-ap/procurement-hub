import { useQuery } from '@tanstack/react-query';
import { useNavigate } from 'react-router-dom';
import { Receipt } from 'lucide-react';
import { DataTable, type DataTableColumn } from '@/shared/components/DataTable';
import { fulfillmentApi, type InvoiceListDto, type InvoiceStatus } from '../api/fulfillmentApi';
import { fmtDate } from '@/shared/lib/date';
import { AuditCell } from '@/shared/components/AuditCell';
import { useBaseCurrency } from '@/shared/hooks/useBaseCurrency';

const STATUS_COLOR: Record<InvoiceStatus, string> = {
  Submitted:   'bg-blue-50 text-blue-700',
  UnderReview: 'bg-yellow-50 text-yellow-700',
  Approved:    'bg-emerald-50 text-emerald-700',
  Paid:        'bg-purple-50 text-purple-700',
  Rejected:    'bg-red-50 text-red-600',
};

const fmt = (n: number) =>
  new Intl.NumberFormat('id-ID', { minimumFractionDigits: 0 }).format(n);

export default function InvoiceListPage() {
  const navigate = useNavigate();
  const base     = useBaseCurrency();
  const sym      = base?.symbol ?? base?.code ?? '?';

  const { data: invoices = [], isLoading } = useQuery({
    queryKey: ['invoices'],
    queryFn:  fulfillmentApi.getInvoiceList,
  });

  const columns: DataTableColumn<InvoiceListDto>[] = [
    { key: 'invoiceNumber', header: 'Invoice No.', sortable: true },
    { key: 'poNumber',      header: 'PO Number',   sortable: true },
    { key: 'vendorName',    header: 'Vendor',       sortable: true },
    {
      key: 'status',
      header: 'Status',
      render: (row) => (
        <span className={`inline-flex px-2 py-0.5 rounded-full text-xs font-medium ${STATUS_COLOR[row.status]}`}>
          {row.status}
        </span>
      ),
    },
    {
      key: 'totalAmount',
      header: `Total (${sym})`,
      render: (row) => fmt(row.totalAmount),
    },
    {
      key: 'dueAt',
      header: 'Due Date',
      render: (row) => fmtDate(row.dueAt),
    },
    {
      key: 'submittedAt',
      header: 'Submitted',
      render: (row) => fmtDate(row.submittedAt),
    },
    {
      key: 'createdAt',
      header: 'Created',
      render: (row) => <AuditCell name={row.createdByName} at={row.createdAt} />,
    },
  ];

  return (
    <div className="space-y-4">
      <div className="flex items-center gap-2 mb-2">
        <Receipt className="h-5 w-5 text-muted-foreground flex-shrink-0" />
        <div>
          <h1 className="text-xl sm:text-2xl font-semibold">Invoices</h1>
          <p className="text-sm text-muted-foreground hidden sm:block">Review and process vendor invoices</p>
        </div>
      </div>

      <DataTable
        data={invoices}
        columns={columns}
        loading={isLoading}
        rowKey="id"
        searchPlaceholder="Search invoice or vendor…"
        onRowClick={(row) => navigate(`/app/fulfillment/invoices/${row.id}`)}
        emptyMessage="No invoices found."
      />
    </div>
  );
}
