import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { Eye, CheckCircle, PauseCircle, Ban, RotateCcw, Plus } from 'lucide-react';
import { toast } from 'sonner';
import { Button } from '@/components/ui/button';
import { DataTable, type DataTableColumn } from '@/shared/components/DataTable';
import { vendorApi, type VendorDto, type VendorStatus } from '../api/vendorApi';
import { VendorFormModal } from './VendorFormModal';

const COMPANY_ID = '00000000-0000-0000-0000-000000000001'; // TODO: from auth context

const StatusBadge = ({ status }: { status: VendorStatus }) => {
  const cfg: Record<VendorStatus, string> = {
    Pending:     'bg-amber-50 text-amber-700',
    Active:      'bg-emerald-50 text-emerald-700',
    Suspended:   'bg-orange-50 text-orange-700',
    Blacklisted: 'bg-red-50 text-red-700',
  };
  return (
    <span className={`inline-flex items-center px-2 py-0.5 rounded-full text-xs font-medium ${cfg[status]}`}>
      {status}
    </span>
  );
};

export default function VendorListPage() {
  const navigate       = useNavigate();
  const qc             = useQueryClient();
  const [showAdd, setShowAdd] = useState(false);

  const { data = [], isLoading } = useQuery({
    queryKey: ['vendors', COMPANY_ID],
    queryFn: () => vendorApi.getAll(COMPANY_ID),
  });

  const mutOpts = (action: string) => ({
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['vendors'] });
      toast.success(`Vendor ${action} successfully`);
    },
    onError: () => toast.error(`Failed to ${action} vendor`),
  });

  const approveMut   = useMutation({ mutationFn: vendorApi.approve,   ...mutOpts('approved') });
  const suspendMut   = useMutation({ mutationFn: vendorApi.suspend,   ...mutOpts('suspended') });
  const reinstateMut = useMutation({ mutationFn: vendorApi.reinstate, ...mutOpts('reinstated') });
  const blacklistMut = useMutation({
    mutationFn: ({ id, reason }: { id: string; reason: string }) => vendorApi.blacklist(id, reason),
    ...mutOpts('blacklisted'),
  });

  const columns: DataTableColumn<VendorDto>[] = [
    {
      key: 'vendorCode',
      header: 'Code',
      sortable: true,
      render: (row) => <span className="font-mono text-sm">{row.vendorCode}</span>,
    },
    { key: 'legalName', header: 'Legal Name', sortable: true },
    {
      key: 'vendorType',
      header: 'Type',
      render: (row) => (
        <span className="text-xs text-slate-500">{row.vendorType}</span>
      ),
    },
    {
      key: 'tier',
      header: 'Tier',
      render: (row) => (
        <span className="text-xs font-medium text-blue-600">{row.tier}</span>
      ),
    },
    {
      key: 'score',
      header: 'Score',
      sortable: true,
      render: (row) => <span className="font-medium">{row.score.toFixed(1)}</span>,
    },
    {
      key: 'status',
      header: 'Status',
      render: (row) => <StatusBadge status={row.status} />,
    },
  ];

  return (
    <div>
      <div className="flex items-center justify-between mb-6">
        <div>
          <h1 className="text-xl font-semibold text-slate-900">Vendors</h1>
          <p className="text-sm text-slate-500 mt-0.5">Manage vendor registrations and approvals</p>
        </div>
        <Button onClick={() => setShowAdd(true)} className="gap-2">
          <Plus className="h-4 w-4" /> Add Vendor
        </Button>
      </div>

      <DataTable
        data={data as unknown as Record<string, unknown>[]}
        columns={columns as DataTableColumn<Record<string, unknown>>[]}
        isLoading={isLoading}
        searchPlaceholder="Search vendors..."
        emptyMessage="No vendors found."
        rowActions={(row) => {
          const v = row as unknown as VendorDto;
          return (
            <div className="flex items-center gap-1">
              <Button
                variant="ghost" size="icon" className="h-8 w-8"
                title="View detail"
                onClick={() => navigate(v.id)}
              >
                <Eye className="h-3.5 w-3.5" />
              </Button>
              {v.status === 'Pending' && (
                <Button
                  variant="ghost" size="icon" className="h-8 w-8 text-emerald-600 hover:text-emerald-700"
                  title="Approve"
                  onClick={() => approveMut.mutate(v.id)}
                >
                  <CheckCircle className="h-3.5 w-3.5" />
                </Button>
              )}
              {v.status === 'Active' && (
                <>
                  <Button
                    variant="ghost" size="icon" className="h-8 w-8 text-orange-500 hover:text-orange-600"
                    title="Suspend"
                    onClick={() => { if (confirm('Suspend this vendor?')) suspendMut.mutate(v.id); }}
                  >
                    <PauseCircle className="h-3.5 w-3.5" />
                  </Button>
                  <Button
                    variant="ghost" size="icon" className="h-8 w-8 text-red-500 hover:text-red-600"
                    title="Blacklist"
                    onClick={() => {
                      const reason = prompt('Enter blacklist reason (required):');
                      if (reason?.trim()) blacklistMut.mutate({ id: v.id, reason: reason.trim() });
                    }}
                  >
                    <Ban className="h-3.5 w-3.5" />
                  </Button>
                </>
              )}
              {(v.status === 'Suspended' || v.status === 'Blacklisted') && (
                <Button
                  variant="ghost" size="icon" className="h-8 w-8 text-blue-500 hover:text-blue-600"
                  title="Reinstate"
                  onClick={() => { if (confirm('Reinstate this vendor?')) reinstateMut.mutate(v.id); }}
                >
                  <RotateCcw className="h-3.5 w-3.5" />
                </Button>
              )}
            </div>
          );
        }}
      />
      <VendorFormModal open={showAdd} onClose={() => setShowAdd(false)} />
    </div>
  );
}
