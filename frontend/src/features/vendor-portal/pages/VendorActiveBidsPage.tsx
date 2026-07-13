import { useNavigate } from 'react-router-dom';
import { useQuery } from '@tanstack/react-query';
import { Eye, Clock, FileText } from 'lucide-react';
import { Button } from '@/components/ui/button';
import { DataTable, type DataTableColumn } from '@/shared/components/DataTable';
import { procurementApi, type RFQListDto, type RFQStatus } from '@/features/procurement/api/procurementApi';
import { fmtDateTime } from '@/shared/lib/date';

const COMPANY_ID = '00000000-0000-0000-0000-000000000001';

const StatusBadge = ({ status }: { status: RFQStatus }) => {
  const cfg: Record<RFQStatus, string> = {
    Draft:     'bg-slate-100 text-slate-700',
    Open:      'bg-blue-50 text-blue-700',
    Closed:    'bg-gray-100 text-gray-500',
    Cancelled: 'bg-red-50 text-red-500',
  };
  return <span className={`inline-flex px-2 py-0.5 rounded-full text-xs font-medium ${cfg[status]}`}>{status}</span>;
};

const DeadlineCell = ({ deadline }: { deadline: string }) => {
  const hoursLeft = (new Date(deadline).getTime() - Date.now()) / 3_600_000;
  const urgent    = hoursLeft > 0 && hoursLeft <= 24;
  return (
    <span className={`flex items-center gap-1 text-sm ${urgent ? 'text-amber-600 font-semibold' : ''}`}>
      {urgent && <Clock className="h-3.5 w-3.5" />}
      {fmtDateTime(deadline)}
    </span>
  );
};

export default function VendorActiveBidsPage() {
  const navigate = useNavigate();

  const { data = [], isLoading } = useQuery({
    queryKey: ['vendor-rfqs', COMPANY_ID],
    queryFn:  () => procurementApi.listRFQs(COMPANY_ID).then(r =>
      r.data.filter(rfq => rfq.status === 'Open')),
  });

  const columns: DataTableColumn<RFQListDto>[] = [
    { key: 'rfqNumber', label: 'RFQ Number', sortable: true },
    { key: 'title',     label: 'Title',       sortable: true },
    {
      key:    'bidDeadline',
      label:  'Bid Deadline',
      sortable: true,
      render: (v: RFQListDto) => <DeadlineCell deadline={v.bidDeadline} />,
    },
    {
      key:    'status',
      label:  'Status',
      sortable: false,
      render: (v: RFQListDto) => <StatusBadge status={v.status} />,
    },
    { key: 'itemCount', label: 'Items', sortable: false },
    {
      key:    'actions' as keyof RFQListDto,
      label:  'Actions',
      sortable: false,
      render: (v: RFQListDto) => (
        <Button size="sm" variant="ghost" onClick={() => navigate(`/vendor/bids/${v.id}`)}>
          <Eye className="h-3.5 w-3.5 mr-1" /> View & Bid
        </Button>
      ),
    },
  ];

  return (
    <div className="space-y-6">
      <div className="flex items-center gap-2">
        <FileText className="h-5 w-5 text-muted-foreground flex-shrink-0" />
        <div>
          <h1 className="text-xl sm:text-2xl font-semibold">Active Bids</h1>
          <p className="text-sm text-muted-foreground hidden sm:block">Open RFQs you have been invited to bid on</p>
        </div>
      </div>
      <DataTable columns={columns} data={data} loading={isLoading} searchable />
    </div>
  );
}
