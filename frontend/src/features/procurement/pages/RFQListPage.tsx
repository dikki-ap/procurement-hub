import { useNavigate } from 'react-router-dom';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { Eye, Plus, Lock } from 'lucide-react';
import { toast } from 'sonner';
import { Button } from '@/components/ui/button';
import { DataTable, type DataTableColumn } from '@/shared/components/DataTable';
import { procurementApi, type RFQListDto, type RFQStatus } from '../api/procurementApi';

const COMPANY_ID = '00000000-0000-0000-0000-000000000001';

const StatusBadge = ({ status }: { status: RFQStatus }) => {
  const cfg: Record<RFQStatus, string> = {
    Draft:     'bg-slate-100 text-slate-700',
    Open:      'bg-blue-50 text-blue-700',
    Closed:    'bg-emerald-50 text-emerald-700',
    Cancelled: 'bg-gray-100 text-gray-500',
  };
  return (
    <span className={`inline-flex items-center px-2 py-0.5 rounded-full text-xs font-medium ${cfg[status]}`}>
      {status}
    </span>
  );
};

export default function RFQListPage() {
  const navigate = useNavigate();
  const qc       = useQueryClient();

  const { data = [], isLoading } = useQuery({
    queryKey: ['rfqs', COMPANY_ID],
    queryFn:  () => procurementApi.listRFQs(COMPANY_ID).then(r => r.data),
  });

  const closeMut = useMutation({
    mutationFn: (id: string) => procurementApi.closeRFQ(id),
    onSuccess:  () => { qc.invalidateQueries({ queryKey: ['rfqs'] }); toast.success('RFQ closed'); },
    onError:    () => toast.error('Failed to close RFQ'),
  });

  const columns: DataTableColumn<RFQListDto>[] = [
    { key: 'rfqNumber',  label: 'RFQ Number', sortable: true },
    { key: 'title',      label: 'Title',       sortable: true },
    {
      key:    'bidDeadline',
      label:  'Bid Deadline',
      sortable: true,
      render: (v: RFQListDto) => new Date(v.bidDeadline).toLocaleDateString('id-ID'),
    },
    {
      key:    'status',
      label:  'Status',
      sortable: true,
      render: (v: RFQListDto) => <StatusBadge status={v.status} />,
    },
    { key: 'itemCount',   label: 'Items',   sortable: false },
    { key: 'vendorCount', label: 'Vendors', sortable: false },
    {
      key:    'actions' as keyof RFQListDto,
      label:  'Actions',
      sortable: false,
      render: (v: RFQListDto) => (
        <div className="flex gap-1">
          <Button size="sm" variant="ghost" onClick={() => navigate(`/procurement/rfqs/${v.id}`)}>
            <Eye className="h-3.5 w-3.5" />
          </Button>
          {v.status === 'Open' && (
            <Button size="sm" variant="outline" onClick={() => closeMut.mutate(v.id)}>
              <Lock className="h-3.5 w-3.5 mr-1" /> Close
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
          <h1 className="text-2xl font-semibold tracking-tight">Request for Quotations</h1>
          <p className="text-sm text-muted-foreground">Manage bidding rounds and vendor invitations</p>
        </div>
        <Button onClick={() => navigate('/procurement/rfqs/new')}>
          <Plus className="h-4 w-4 mr-2" /> New RFQ
        </Button>
      </div>
      <DataTable columns={columns} data={data} loading={isLoading} searchable />
    </div>
  );
}
