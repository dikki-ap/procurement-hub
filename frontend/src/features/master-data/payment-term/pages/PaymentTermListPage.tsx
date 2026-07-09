import { useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { Plus, Pencil, Trash2 } from 'lucide-react';
import { toast } from 'sonner';
import { Button } from '@/components/ui/button';
import { DataTable, type DataTableColumn } from '@/shared/components/DataTable';
import { ConfirmDeleteModal } from '@/shared/components/ConfirmDeleteModal';
import { PaymentTermFormModal } from './PaymentTermFormModal';
import { useAuthStore } from '@/stores/authStore';
import { paymentTermApi, type PaymentTermDto } from '../api/paymentTermApi';

type ModalState = { mode: 'add' | 'edit'; id?: string } | null;

const StatusBadge = ({ active }: { active: boolean }) => (
  <span
    className={`inline-flex items-center px-2 py-0.5 rounded-full text-xs font-medium ${
      active ? 'bg-emerald-50 text-emerald-700' : 'bg-slate-100 text-slate-500'
    }`}
  >
    {active ? 'Active' : 'Inactive'}
  </span>
);

export default function PaymentTermListPage() {
  const qc = useQueryClient();
  const companyId = useAuthStore((s) => s.user?.companyId ?? '');
  const [modal, setModal] = useState<ModalState>(null);
  const [deleteTarget, setDeleteTarget] = useState<{ id: string; name: string } | null>(null);

  const { data = [], isLoading } = useQuery({
    queryKey: ['payment-terms', companyId],
    queryFn: () => paymentTermApi.getAll(companyId),
    enabled: !!companyId,
  });

  const deleteMut = useMutation({
    mutationFn: paymentTermApi.delete,
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['payment-terms'] });
      toast.success('Payment term deleted', { duration: 3000 });
      setDeleteTarget(null);
    },
    onError: () => {
      toast.error('Delete failed');
      setDeleteTarget(null);
    },
  });

  const columns: DataTableColumn<PaymentTermDto>[] = [
    {
      key: 'code',
      header: 'Code',
      sortable: true,
      render: (row) => <span className="font-mono font-medium">{row.code}</span>,
    },
    { key: 'name', header: 'Name', sortable: true },
    {
      key: 'days',
      header: 'Days',
      sortable: true,
      render: (row) => <span className="tabular-nums">{row.days}</span>,
    },
    {
      key: 'description',
      header: 'Description',
      render: (row) => row.description ?? '-',
    },
    {
      key: 'isActive',
      header: 'Status',
      render: (row) => <StatusBadge active={row.isActive} />,
    },
  ];

  return (
    <div>
      <div className="flex items-center justify-between mb-6">
        <div>
          <h1 className="text-xl font-semibold text-slate-900">Payment Terms</h1>
          <p className="text-sm text-slate-500 mt-0.5">Define payment conditions and due days</p>
        </div>
        <Button onClick={() => setModal({ mode: 'add' })} className="gap-2">
          <Plus className="h-4 w-4" /> Add Payment Term
        </Button>
      </div>

      <DataTable
        data={data as unknown as Record<string, unknown>[]}
        columns={columns as DataTableColumn<Record<string, unknown>>[]}
        isLoading={isLoading}
        searchPlaceholder="Search payment terms..."
        emptyMessage="No payment terms found."
        rowActions={(row) => {
          const term = row as unknown as PaymentTermDto;
          return (
            <>
              <Button
                variant="ghost"
                size="icon"
                className="h-8 w-8"
                onClick={() => setModal({ mode: 'edit', id: term.id })}
              >
                <Pencil className="h-3.5 w-3.5" />
              </Button>
              <Button
                variant="ghost"
                size="icon"
                className="h-8 w-8 text-red-500 hover:text-red-600"
                onClick={() => setDeleteTarget({ id: term.id, name: term.code })}
              >
                <Trash2 className="h-3.5 w-3.5" />
              </Button>
            </>
          );
        }}
      />

      <PaymentTermFormModal
        open={modal !== null}
        id={modal?.mode === 'edit' ? modal.id : undefined}
        onClose={() => setModal(null)}
      />

      <ConfirmDeleteModal
        open={deleteTarget !== null}
        title="Delete Payment Term"
        description={`Delete "${deleteTarget?.name}"? This action cannot be undone.`}
        isPending={deleteMut.isPending}
        onConfirm={() => deleteTarget && deleteMut.mutate(deleteTarget.id)}
        onCancel={() => setDeleteTarget(null)}
      />
    </div>
  );
}
