import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { Eye, Plus, XCircle } from 'lucide-react';
import { toast } from 'sonner';
import { Button } from '@/components/ui/button';
import { DataTable, type DataTableColumn } from '@/shared/components/DataTable';
import { procurementApi, type PRListDto, type PRStatus } from '../api/procurementApi';
import { NewPRModal } from './NewPRModal';

const COMPANY_ID = '00000000-0000-0000-0000-000000000001';

const StatusBadge = ({ status }: { status: PRStatus }) => {
  const cfg: Record<PRStatus, string> = {
    Draft:     'bg-slate-100 text-slate-700',
    Submitted: 'bg-blue-50 text-blue-700',
    Approved:  'bg-emerald-50 text-emerald-700',
    Rejected:  'bg-red-50 text-red-700',
    Cancelled: 'bg-gray-100 text-gray-500',
  };
  return (
    <span className={`inline-flex items-center px-2 py-0.5 rounded-full text-xs font-medium ${cfg[status]}`}>
      {status}
    </span>
  );
};

const fmt = new Intl.NumberFormat('id-ID', { style: 'currency', currency: 'IDR', maximumFractionDigits: 0 });

export default function PRListPage() {
  const navigate   = useNavigate();
  const qc         = useQueryClient();
  const [showNew, setShowNew] = useState(false);

  const { data = [], isLoading } = useQuery({
    queryKey: ['prs', COMPANY_ID],
    queryFn:  () => procurementApi.listPRs(COMPANY_ID),
  });

  const submitMut = useMutation({
    mutationFn: (id: string) => procurementApi.submitPR(id),
    onSuccess:  () => { qc.invalidateQueries({ queryKey: ['prs'] }); toast.success('PR submitted'); },
    onError:    () => toast.error('Failed to submit PR'),
  });

  const cancelMut = useMutation({
    mutationFn: (id: string) => procurementApi.cancelPR(id),
    onSuccess:  () => { qc.invalidateQueries({ queryKey: ['prs'] }); toast.success('PR cancelled'); },
    onError:    () => toast.error('Failed to cancel PR'),
  });

  const columns: DataTableColumn<PRListDto>[] = [
    { key: 'prNumber',  header: 'PR Number',   sortable: true },
    { key: 'title',     header: 'Title',        sortable: true },
    { key: 'department', header: 'Department',  sortable: true },
    {
      key:    'requiredDate',
      header:  'Required Date',
      sortable: true,
      render: (v: PRListDto) => new Date(v.requiredDate).toLocaleDateString('id-ID'),
    },
    {
      key:    'status',
      header:  'Status',
      sortable: true,
      render: (v: PRListDto) => <StatusBadge status={v.status} />,
    },
    {
      key:    'totalEstimatedValue',
      header:  'Est. Value',
      sortable: true,
      render: (v: PRListDto) => fmt.format(v.totalEstimatedValue),
    },
    { key: 'itemCount', header: 'Items', sortable: false },
    {
      key:    'actions' as keyof PRListDto,
      header:  'Actions',
      sortable: false,
      render: (v: PRListDto) => (
        <div className="flex gap-1">
          <Button size="sm" variant="ghost" onClick={() => navigate(`/procurement/prs/${v.id}`)}>
            <Eye className="h-3.5 w-3.5" />
          </Button>
          {v.status === 'Draft' && (
            <Button size="sm" variant="outline" onClick={() => submitMut.mutate(v.id)}>
              Submit
            </Button>
          )}
          {(v.status === 'Draft' || v.status === 'Submitted') && (
            <Button size="sm" variant="ghost" className="text-red-600"
              onClick={() => cancelMut.mutate(v.id)}>
              <XCircle className="h-3.5 w-3.5" />
            </Button>
          )}
        </div>
      ),
    },
  ];

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-semibold tracking-tight">Purchase Requisitions</h1>
          <p className="text-sm text-muted-foreground">Manage and track all purchase requests</p>
        </div>
        <Button onClick={() => setShowNew(true)}>
          <Plus className="h-4 w-4 mr-2" /> New PR
        </Button>
      </div>
      <DataTable columns={columns} data={data as unknown as Record<string, unknown>[]} isLoading={isLoading} searchPlaceholder="Search PRs..." />
      <NewPRModal open={showNew} onClose={() => setShowNew(false)} />
    </div>
  );
}
