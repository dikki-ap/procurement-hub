import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { Eye, CheckCircle, PauseCircle, Ban, RotateCcw, Plus, Users, Download } from 'lucide-react';
import { toast } from 'sonner';
import { downloadExcel } from '@/shared/lib/downloadFile';
import { Button } from '@/components/ui/button';
import { DataTable, type DataTableColumn } from '@/shared/components/DataTable';
import { vendorApi, type VendorDto, type VendorStatus } from '../api/vendorApi';
import { VendorFormModal } from './VendorFormModal';
import { TierBadge, ScoreBadge } from '../components/VendorBadges';
import { SuspendModal, BlacklistModal, ReinstateModal } from '../components/VendorActionModals';
import { extractApiError } from '@/shared/lib/apiError';
import { AuditCell } from '@/shared/components/AuditCell';
import { useAuthStore } from '@/stores/authStore';

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

type ActionModal =
  | { type: 'suspend';   vendor: VendorDto }
  | { type: 'blacklist'; vendor: VendorDto }
  | { type: 'reinstate'; vendor: VendorDto }
  | null;

export default function VendorListPage() {
  const navigate   = useNavigate();
  const qc         = useQueryClient();
  const companyId  = useAuthStore(s => s.user?.companyId ?? '');

  const [showAdd,     setShowAdd]     = useState(false);
  const [modal,       setModal]       = useState<ActionModal>(null);
  const [isExporting, setIsExporting] = useState(false);

  const { data = [], isLoading } = useQuery({
    queryKey: ['vendors', companyId],
    queryFn: () => vendorApi.getAll(companyId),
  });

  const closeModal = () => setModal(null);

  const handleExport = async () => {
    setIsExporting(true);
    try {
      const today = new Date().toISOString().slice(0, 10).replace(/-/g, '');
      await downloadExcel(`/vendors/export?companyId=${companyId}`, `Vendors_${today}.xlsx`);
    } catch {
      toast.error('Failed to export vendors');
    } finally {
      setIsExporting(false);
    }
  };

  const mutOpts = (action: string) => ({
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['vendors'] });
      qc.invalidateQueries({ queryKey: ['vendor'] });
      toast.success(`Vendor ${action} successfully`);
      closeModal();
    },
    onError: (error: unknown) => {
      toast.error(extractApiError(error, `Failed to ${action} vendor`));
      closeModal();
    },
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
      render: (row) => <span className="text-xs text-slate-500">{row.vendorType}</span>,
    },
    {
      key: 'tier',
      header: 'Tier',
      render: (row) => <TierBadge tier={row.tier} />,
    },
    {
      key: 'score',
      header: 'Score',
      sortable: true,
      render: (row) => <ScoreBadge score={row.score} />,
    },
    {
      key: 'status',
      header: 'Status',
      render: (row) => <StatusBadge status={row.status} />,
    },
    {
      key: 'createdAt',
      header: 'Created',
      render: (row) => <AuditCell name={row.createdByName} at={row.createdAt} />,
    },
    {
      key: 'updatedAt',
      header: 'Last Modified',
      render: (row) => <AuditCell name={row.updatedByName} at={row.updatedAt} />,
    },
  ];

  const activeModal = modal?.vendor;

  return (
    <div>
      <div className="flex flex-wrap items-center justify-between gap-3 mb-6">
        <div className="flex items-center gap-2">
          <Users className="h-5 w-5 text-muted-foreground flex-shrink-0" />
          <div>
            <h1 className="text-xl sm:text-2xl font-semibold">Vendors</h1>
            <p className="text-sm text-muted-foreground hidden sm:block">Manage vendor registrations and approvals</p>
          </div>
        </div>
        <div className="flex gap-2">
          <Button variant="outline" size="sm" onClick={handleExport} disabled={isExporting} className="gap-1">
            <Download className="h-4 w-4" />
            <span className="hidden sm:inline">{isExporting ? 'Exporting...' : 'Export'}</span>
          </Button>
          <Button size="sm" onClick={() => setShowAdd(true)} className="gap-1">
            <Plus className="h-4 w-4" /> Add Vendor
          </Button>
        </div>
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
                    onClick={() => setModal({ type: 'suspend', vendor: v })}
                  >
                    <PauseCircle className="h-3.5 w-3.5" />
                  </Button>
                  <Button
                    variant="ghost" size="icon" className="h-8 w-8 text-red-500 hover:text-red-600"
                    title="Blacklist"
                    onClick={() => setModal({ type: 'blacklist', vendor: v })}
                  >
                    <Ban className="h-3.5 w-3.5" />
                  </Button>
                </>
              )}
              {(v.status === 'Suspended' || v.status === 'Blacklisted') && (
                <Button
                  variant="ghost" size="icon" className="h-8 w-8 text-blue-500 hover:text-blue-600"
                  title="Reinstate"
                  onClick={() => setModal({ type: 'reinstate', vendor: v })}
                >
                  <RotateCcw className="h-3.5 w-3.5" />
                </Button>
              )}
            </div>
          );
        }}
      />

      <VendorFormModal open={showAdd} onClose={() => setShowAdd(false)} />

      <SuspendModal
        open={modal?.type === 'suspend'}
        vendorName={activeModal?.legalName ?? ''}
        isPending={suspendMut.isPending}
        onConfirm={() => activeModal && suspendMut.mutate(activeModal.id)}
        onCancel={closeModal}
      />

      <BlacklistModal
        open={modal?.type === 'blacklist'}
        vendorName={activeModal?.legalName ?? ''}
        isPending={blacklistMut.isPending}
        onConfirm={(reason) => activeModal && blacklistMut.mutate({ id: activeModal.id, reason })}
        onCancel={closeModal}
      />

      <ReinstateModal
        open={modal?.type === 'reinstate'}
        vendorName={activeModal?.legalName ?? ''}
        isPending={reinstateMut.isPending}
        onConfirm={() => activeModal && reinstateMut.mutate(activeModal.id)}
        onCancel={closeModal}
      />
    </div>
  );
}
