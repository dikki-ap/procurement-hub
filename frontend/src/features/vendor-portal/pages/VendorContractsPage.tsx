import { useParams } from 'react-router-dom';
import { useQuery } from '@tanstack/react-query';
import { FileText } from 'lucide-react';
import { DataTable, type DataTableColumn } from '@/shared/components/DataTable';
import { fmtDate } from '@/shared/lib/date';
import { contractApi, type ContractListDto, type ContractStatus } from '@/features/contracts/api/contractApi';

const STATUS_CFG: Record<ContractStatus, string> = {
  Draft:      'bg-slate-100 text-slate-600',
  Active:     'bg-emerald-50 text-emerald-700',
  Expired:    'bg-amber-50 text-amber-700',
  Terminated: 'bg-red-50 text-red-700',
};

const StatusBadge = ({ status }: { status: ContractStatus }) => (
  <span className={`inline-flex items-center px-2 py-0.5 rounded-full text-xs font-medium ${STATUS_CFG[status]}`}>
    {status}
  </span>
);

export default function VendorContractsPage() {
  const { vendorId } = useParams<{ vendorId: string }>();

  const { data: contracts = [], isLoading } = useQuery({
    queryKey: ['vendor-contracts', vendorId],
    queryFn: () => contractApi.getByVendor(vendorId!),
    enabled: !!vendorId,
  });

  const columns: DataTableColumn<ContractListDto>[] = [
    {
      key: 'contractNumber',
      header: 'Contract #',
      sortable: true,
      render: (row) => (
        <span className="font-mono text-sm font-semibold text-slate-800">{row.contractNumber}</span>
      ),
    },
    {
      key: 'title',
      header: 'Title',
      sortable: true,
      render: (row) => <span className="text-sm font-medium text-slate-900">{row.title}</span>,
    },
    {
      key: 'status',
      header: 'Status',
      render: (row) => <StatusBadge status={row.status} />,
    },
    {
      key: 'startDate',
      header: 'Start',
      render: (row) => <span className="text-sm text-slate-600">{fmtDate(row.startDate)}</span>,
    },
    {
      key: 'endDate',
      header: 'End',
      render: (row) => {
        const isExpiring = row.status === 'Active' && row.endDate
          ? (new Date(row.endDate).getTime() - Date.now()) / 86400000 <= 30
          : false;
        return (
          <span className={`text-sm ${isExpiring ? 'text-amber-600 font-medium' : 'text-slate-600'}`}>
            {fmtDate(row.endDate)}
          </span>
        );
      },
    },
    {
      key: 'value',
      header: 'Value',
      render: (row) => (
        <span className="text-sm text-slate-600">
          {row.value != null ? row.value.toLocaleString(undefined, { maximumFractionDigits: 0 }) : '—'}
        </span>
      ),
    },
  ];

  return (
    <div>
      <div className="flex items-center gap-2 mb-6">
        <FileText className="h-5 w-5 text-muted-foreground flex-shrink-0" />
        <div>
          <h1 className="text-xl sm:text-2xl font-semibold">My Contracts</h1>
          <p className="text-sm text-muted-foreground hidden sm:block">
            Your active and historical contracts
          </p>
        </div>
      </div>

      <DataTable
        data={contracts}
        columns={columns}
        loading={isLoading}
        rowKey="id"
        emptyMessage="No contracts found."
      />
    </div>
  );
}
