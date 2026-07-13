import { useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { Plus, Pencil, Trash2, Package } from 'lucide-react';
import { toast } from 'sonner';
import { Button } from '@/components/ui/button';
import { DataTable, type DataTableColumn } from '@/shared/components/DataTable';
import { ConfirmDeleteModal } from '@/shared/components/ConfirmDeleteModal';
import { MaterialFormModal } from './MaterialFormModal';
import { materialApi, type MaterialDto } from '../api/materialApi';
import { AuditCell } from '@/shared/components/AuditCell';

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

export default function MaterialListPage() {
  const qc = useQueryClient();
  const [modal, setModal] = useState<ModalState>(null);
  const [deleteTarget, setDeleteTarget] = useState<{ id: string; name: string } | null>(null);

  const { data = [], isLoading } = useQuery({
    queryKey: ['materials'],
    queryFn: materialApi.getAll,
  });

  const deleteMut = useMutation({
    mutationFn: materialApi.delete,
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['materials'] });
      toast.success('Material deleted', { duration: 3000 });
      setDeleteTarget(null);
    },
    onError: () => {
      toast.error('Delete failed');
      setDeleteTarget(null);
    },
  });

  const columns: DataTableColumn<MaterialDto>[] = [
    {
      key: 'code',
      header: 'Code',
      sortable: true,
      render: (row) => <span className="font-mono font-medium">{row.code}</span>,
    },
    { key: 'name', header: 'Name', sortable: true },
    {
      key: 'categoryName',
      header: 'Category',
      sortable: true,
      render: (row) => row.categoryName ?? '-',
    },
    {
      key: 'uomCode',
      header: 'UOM',
      render: (row) => <span className="font-mono text-xs">{row.uomCode}</span>,
    },
    {
      key: 'estimatedPrice',
      header: 'Est. Price',
      sortable: true,
      render: (row) =>
        row.estimatedPrice != null
          ? `${row.currencyCode ?? ''} ${row.estimatedPrice.toLocaleString('id-ID')}`.trim()
          : '-',
    },
    {
      key: 'isActive',
      header: 'Status',
      render: (row) => <StatusBadge active={row.isActive} />,
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

  return (
    <div>
      <div className="flex flex-wrap items-center justify-between gap-3 mb-6">
        <div className="flex items-center gap-2">
          <Package className="h-5 w-5 text-muted-foreground flex-shrink-0" />
          <div>
            <h1 className="text-xl sm:text-2xl font-semibold">Materials</h1>
            <p className="text-sm text-muted-foreground hidden sm:block">Manage your material master data catalog</p>
          </div>
        </div>
        <Button size="sm" onClick={() => setModal({ mode: 'add' })} className="gap-1">
          <Plus className="h-4 w-4" /> Add Material
        </Button>
      </div>

      <DataTable
        data={data as unknown as Record<string, unknown>[]}
        columns={columns as DataTableColumn<Record<string, unknown>>[]}
        isLoading={isLoading}
        searchPlaceholder="Search materials..."
        emptyMessage="No materials found."
        rowActions={(row) => {
          const material = row as unknown as MaterialDto;
          return (
            <>
              <Button
                variant="ghost"
                size="icon"
                className="h-8 w-8"
                onClick={() => setModal({ mode: 'edit', id: material.id })}
              >
                <Pencil className="h-3.5 w-3.5" />
              </Button>
              <Button
                variant="ghost"
                size="icon"
                className="h-8 w-8 text-red-500 hover:text-red-600"
                onClick={() => setDeleteTarget({ id: material.id, name: material.code })}
              >
                <Trash2 className="h-3.5 w-3.5" />
              </Button>
            </>
          );
        }}
      />

      <MaterialFormModal
        open={modal !== null}
        id={modal?.mode === 'edit' ? modal.id : undefined}
        onClose={() => setModal(null)}
      />

      <ConfirmDeleteModal
        open={deleteTarget !== null}
        title="Delete Material"
        description={`Delete "${deleteTarget?.name}"? This action cannot be undone.`}
        isPending={deleteMut.isPending}
        onConfirm={() => deleteTarget && deleteMut.mutate(deleteTarget.id)}
        onCancel={() => setDeleteTarget(null)}
      />
    </div>
  );
}
