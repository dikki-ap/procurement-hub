import { useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { Plus, Pencil, Trash2, FolderOpen } from 'lucide-react';
import { toast } from 'sonner';
import { Button } from '@/components/ui/button';
import { DataTable, type DataTableColumn } from '@/shared/components/DataTable';
import { ConfirmDeleteModal } from '@/shared/components/ConfirmDeleteModal';
import { MaterialCategoryFormModal } from './MaterialCategoryFormModal';
import { useAuthStore } from '@/stores/authStore';
import { materialCategoryApi, type MaterialCategoryDto } from '../api/materialCategoryApi';

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

const StrategicBadge = ({ isStrategic }: { isStrategic: boolean }) =>
  isStrategic ? (
    <span className="inline-flex items-center px-2 py-0.5 rounded-full text-xs font-medium bg-amber-50 text-amber-700">
      Yes
    </span>
  ) : (
    <span className="text-slate-400 text-sm">No</span>
  );

export default function MaterialCategoryListPage() {
  const qc = useQueryClient();
  const companyId = useAuthStore((s) => s.user?.companyId ?? '');
  const [modal, setModal] = useState<ModalState>(null);
  const [deleteTarget, setDeleteTarget] = useState<{ id: string; name: string } | null>(null);

  const { data = [], isLoading } = useQuery({
    queryKey: ['material-categories', companyId],
    queryFn: () => materialCategoryApi.getAll(companyId),
    enabled: !!companyId,
  });

  const deleteMut = useMutation({
    mutationFn: materialCategoryApi.delete,
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['material-categories'] });
      toast.success('Category deleted', { duration: 3000 });
      setDeleteTarget(null);
    },
    onError: () => {
      toast.error('Delete failed');
      setDeleteTarget(null);
    },
  });

  const parentMap = Object.fromEntries(
    (data as MaterialCategoryDto[]).map((c) => [c.id, c.name])
  );

  const columns: DataTableColumn<MaterialCategoryDto>[] = [
    {
      key: 'code',
      header: 'Code',
      sortable: true,
      render: (row) => <span className="font-mono font-medium">{row.code}</span>,
    },
    { key: 'name', header: 'Name', sortable: true },
    {
      key: 'parentId',
      header: 'Parent',
      render: (row) => (row.parentId ? (parentMap[row.parentId] ?? '-') : '-'),
      searchValue: (row) => (row.parentId ? (parentMap[row.parentId] ?? '') : ''),
    },
    {
      key: 'isStrategic',
      header: 'Strategic',
      render: (row) => <StrategicBadge isStrategic={row.isStrategic} />,
    },
    {
      key: 'isActive',
      header: 'Status',
      render: (row) => <StatusBadge active={row.isActive} />,
    },
  ];

  return (
    <div>
      <div className="flex flex-wrap items-center justify-between gap-3 mb-6">
        <div className="flex items-center gap-2">
          <FolderOpen className="h-5 w-5 text-muted-foreground flex-shrink-0" />
          <div>
            <h1 className="text-xl sm:text-2xl font-semibold">Material Categories</h1>
            <p className="text-sm text-muted-foreground hidden sm:block">Organize materials into hierarchical categories</p>
          </div>
        </div>
        <Button size="sm" onClick={() => setModal({ mode: 'add' })} className="gap-1">
          <Plus className="h-4 w-4" /> Add Category
        </Button>
      </div>

      <DataTable
        data={data as unknown as Record<string, unknown>[]}
        columns={columns as DataTableColumn<Record<string, unknown>>[]}
        isLoading={isLoading}
        searchPlaceholder="Search categories..."
        emptyMessage="No categories found."
        rowActions={(row) => {
          const category = row as unknown as MaterialCategoryDto;
          return (
            <>
              <Button
                variant="ghost"
                size="icon"
                className="h-8 w-8"
                onClick={() => setModal({ mode: 'edit', id: category.id })}
              >
                <Pencil className="h-3.5 w-3.5" />
              </Button>
              <Button
                variant="ghost"
                size="icon"
                className="h-8 w-8 text-red-500 hover:text-red-600"
                onClick={() => setDeleteTarget({ id: category.id, name: category.name })}
              >
                <Trash2 className="h-3.5 w-3.5" />
              </Button>
            </>
          );
        }}
      />

      <MaterialCategoryFormModal
        open={modal !== null}
        id={modal?.mode === 'edit' ? modal.id : undefined}
        onClose={() => setModal(null)}
      />

      <ConfirmDeleteModal
        open={deleteTarget !== null}
        title="Delete Category"
        description={`Delete "${deleteTarget?.name}"? This action cannot be undone.`}
        isPending={deleteMut.isPending}
        onConfirm={() => deleteTarget && deleteMut.mutate(deleteTarget.id)}
        onCancel={() => setDeleteTarget(null)}
      />
    </div>
  );
}
