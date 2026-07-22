import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useQuery } from '@tanstack/react-query';
import { FileText, Plus } from 'lucide-react';
import { Button } from '@/components/ui/button';
import { DataTable, type DataTableColumn } from '@/shared/components/DataTable';
import { fmtDate } from '@/shared/lib/date';
import { contractApi, type ContractListDto, type ContractStatus } from '../api/contractApi';
import { NewContractModal } from '../components/NewContractModal';

const STATUS_CFG: Record<ContractStatus, string> = {
  Draft:      'bg-slate-100 text-slate-600',
  Active:     'bg-emerald-50 text-emerald-700',
  Expired:    'bg-amber-50 text-amber-700',
  Terminated: 'bg-red-50 text-red-700',
};

const ContractStatusBadge = ({ status }: { status: ContractStatus }) => (
  <span className={`inline-flex items-center px-2 py-0.5 rounded-full text-xs font-medium ${STATUS_CFG[status]}`}>
    {status}
  </span>
);

const ALL_STATUSES: ContractStatus[] = ['Draft', 'Active', 'Expired', 'Terminated'];

export default function ContractListPage() {
  const navigate = useNavigate();
  const [showNew, setShowNew] = useState(false);
  const [filterStatus, setFilterStatus] = useState<ContractStatus | ''>('');

  const { data: contracts = [], isLoading, refetch } = useQuery({
    queryKey: ['contracts'],
    queryFn: contractApi.getList,
  });

  const filtered = filterStatus
    ? contracts.filter(c => c.status === filterStatus)
    : contracts;

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
      render: (row) => <span className="font-medium text-slate-900">{row.title}</span>,
    },
    {
      key: 'vendorName',
      header: 'Vendor',
      sortable: true,
      render: (row) => <span className="text-sm text-slate-700">{row.vendorName}</span>,
    },
    {
      key: 'status',
      header: 'Status',
      sortable: true,
      render: (row) => <ContractStatusBadge status={row.status} />,
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
    {
      key: 'actions' as keyof ContractListDto,
      header: '',
      render: (row) => (
        <Button
          variant="ghost"
          size="sm"
          className="text-xs text-slate-500 hover:text-indigo-600"
          onClick={() => navigate(`/app/contracts/${row.id}`)}
        >
          View
        </Button>
      ),
    },
  ];

  return (
    <div>
      <div className="flex flex-wrap items-center justify-between gap-3 mb-6">
        <div className="flex items-center gap-2">
          <FileText className="h-5 w-5 text-muted-foreground flex-shrink-0" />
          <div>
            <h1 className="text-xl sm:text-2xl font-semibold">Contracts</h1>
            <p className="text-sm text-muted-foreground hidden sm:block">
              Manage vendor contracts and agreements
            </p>
          </div>
        </div>
        <div className="flex items-center gap-2">
          <select
            className="rounded-md border border-input bg-transparent px-3 py-1.5 text-sm shadow-sm focus-visible:outline-none focus-visible:ring-1 focus-visible:ring-ring"
            value={filterStatus}
            onChange={(e) => setFilterStatus(e.target.value as ContractStatus | '')}
          >
            <option value="">All statuses</option>
            {ALL_STATUSES.map(s => <option key={s} value={s}>{s}</option>)}
          </select>
          <Button size="sm" onClick={() => setShowNew(true)}>
            <Plus className="h-3.5 w-3.5 mr-1" /> New Contract
          </Button>
        </div>
      </div>

      <DataTable
        data={filtered}
        columns={columns}
        loading={isLoading}
        rowKey="id"
        emptyMessage="No contracts found."
      />

      <NewContractModal
        open={showNew}
        onClose={() => setShowNew(false)}
        onCreated={(id) => {
          setShowNew(false);
          refetch();
          navigate(`/app/contracts/${id}`);
        }}
      />
    </div>
  );
}
