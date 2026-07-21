import { useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { Plus, Pencil, Trash2, GitBranch } from 'lucide-react';
import { toast } from 'sonner';
import { Button } from '@/components/ui/button';
import {
  Dialog, DialogContent, DialogHeader, DialogTitle,
} from '@/components/ui/dialog';
import { DataTable, type DataTableColumn } from '@/shared/components/DataTable';
import { ConfirmDeleteModal } from '@/shared/components/ConfirmDeleteModal';
import { useAuthStore } from '@/stores/authStore';
import { departmentApi, type DepartmentDto, type DepartmentRequest } from '../api/departmentApi';
import { extractApiError } from '@/shared/lib/apiError';

const inputCls =
  'w-full rounded-md border border-input bg-transparent px-3 py-2 text-sm shadow-sm transition-colors placeholder:text-muted-foreground focus-visible:outline-none focus-visible:ring-1 focus-visible:ring-ring disabled:cursor-not-allowed disabled:opacity-50';

type ModalState = { mode: 'add' } | { mode: 'edit'; row: DepartmentDto } | null;

const emptyForm = (): DepartmentRequest => ({ name: '', code: '', isActive: true });

const StatusBadge = ({ active }: { active: boolean }) => (
  <span className={`inline-flex items-center px-2 py-0.5 rounded-full text-xs font-medium ${active ? 'bg-emerald-50 text-emerald-700' : 'bg-slate-100 text-slate-500'}`}>
    {active ? 'Active' : 'Inactive'}
  </span>
);

export default function DepartmentListPage() {
  const qc = useQueryClient();
  const companyId = useAuthStore((s) => s.user?.companyId ?? '');
  const [modal, setModal] = useState<ModalState>(null);
  const [form, setForm] = useState<DepartmentRequest>(emptyForm());
  const [deleteTarget, setDeleteTarget] = useState<{ id: string; name: string } | null>(null);

  const { data = [], isLoading } = useQuery({
    queryKey: ['departments', companyId],
    queryFn: () => departmentApi.getAll(companyId),
    enabled: !!companyId,
  });

  const openAdd = () => { setForm(emptyForm()); setModal({ mode: 'add' }); };
  const openEdit = (row: DepartmentDto) => {
    setForm({ name: row.name, code: row.code, isActive: row.isActive });
    setModal({ mode: 'edit', row });
  };
  const closeModal = () => setModal(null);

  const createMut = useMutation({
    mutationFn: () => departmentApi.create(form),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['departments'] });
      toast.success('Department created');
      closeModal();
    },
    onError: (e: unknown) => toast.error(extractApiError(e, 'Failed to create department')),
  });

  const updateMut = useMutation({
    mutationFn: () => departmentApi.update((modal as { mode: 'edit'; row: DepartmentDto }).row.id, form),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['departments'] });
      toast.success('Department updated');
      closeModal();
    },
    onError: (e: unknown) => toast.error(extractApiError(e, 'Failed to update department')),
  });

  const deleteMut = useMutation({
    mutationFn: (id: string) => departmentApi.delete(id),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['departments'] });
      toast.success('Department deleted');
      setDeleteTarget(null);
    },
    onError: (e: unknown) => {
      toast.error(extractApiError(e, 'Cannot delete department'));
      setDeleteTarget(null);
    },
  });

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    modal?.mode === 'edit' ? updateMut.mutate() : createMut.mutate();
  };

  const columns: DataTableColumn<DepartmentDto>[] = [
    {
      key: 'code',
      header: 'Code',
      sortable: true,
      render: (row) => <span className="font-mono text-sm font-semibold text-slate-700">{row.code}</span>,
    },
    {
      key: 'name',
      header: 'Name',
      sortable: true,
      render: (row) => <span className="font-medium text-slate-900">{row.name}</span>,
    },
    {
      key: 'isActive',
      header: 'Status',
      render: (row) => <StatusBadge active={row.isActive} />,
    },
    {
      key: 'actions' as keyof DepartmentDto,
      header: '',
      render: (row) => (
        <div className="flex items-center gap-1 justify-end">
          <Button variant="ghost" size="icon" className="h-7 w-7 text-slate-400 hover:text-blue-600"
            onClick={() => openEdit(row)}>
            <Pencil className="h-3.5 w-3.5" />
          </Button>
          <Button variant="ghost" size="icon" className="h-7 w-7 text-slate-400 hover:text-red-500"
            onClick={() => setDeleteTarget({ id: row.id, name: row.name })}>
            <Trash2 className="h-3.5 w-3.5" />
          </Button>
        </div>
      ),
    },
  ];

  return (
    <div>
      <div className="flex flex-wrap items-center justify-between gap-3 mb-6">
        <div className="flex items-center gap-2">
          <GitBranch className="h-5 w-5 text-muted-foreground flex-shrink-0" />
          <div>
            <h1 className="text-xl sm:text-2xl font-semibold">Departments</h1>
            <p className="text-sm text-muted-foreground hidden sm:block">Manage organizational departments</p>
          </div>
        </div>
        <Button size="sm" onClick={openAdd}>
          <Plus className="h-3.5 w-3.5 mr-1" /> Add Department
        </Button>
      </div>

      <DataTable
        data={data}
        columns={columns}
        loading={isLoading}
        rowKey="id"
        emptyMessage="No departments yet. Add your first department."
      />

      {/* Add / Edit Modal */}
      <Dialog open={!!modal} onOpenChange={(v) => { if (!v) closeModal(); }}>
        <DialogContent className="max-w-sm">
          <DialogHeader>
            <DialogTitle>{modal?.mode === 'edit' ? 'Edit Department' : 'Add Department'}</DialogTitle>
          </DialogHeader>
          <form onSubmit={handleSubmit} className="space-y-4 mt-2">
            <div>
              <label className="block text-sm font-medium mb-1">Name <span className="text-red-500">*</span></label>
              <input
                required
                className={inputCls}
                value={form.name}
                onChange={(e) => setForm(f => ({ ...f, name: e.target.value }))}
                placeholder="e.g. Finance"
              />
            </div>
            <div>
              <label className="block text-sm font-medium mb-1">Code <span className="text-red-500">*</span></label>
              <input
                required
                className={inputCls}
                value={form.code}
                onChange={(e) => setForm(f => ({ ...f, code: e.target.value.toUpperCase() }))}
                placeholder="e.g. FIN"
                maxLength={50}
              />
              <p className="text-xs text-slate-400 mt-0.5">Unique within company, auto-uppercased</p>
            </div>
            <div className="flex items-center gap-2">
              <input
                type="checkbox"
                id="deptActive"
                checked={form.isActive}
                onChange={(e) => setForm(f => ({ ...f, isActive: e.target.checked }))}
                className="h-4 w-4 rounded border-gray-300 text-blue-600 focus:ring-blue-500"
              />
              <label htmlFor="deptActive" className="text-sm font-medium">Active</label>
            </div>
            <div className="flex justify-end gap-2 pt-1">
              <Button type="button" variant="outline" onClick={closeModal}>Cancel</Button>
              <Button type="submit" disabled={createMut.isPending || updateMut.isPending}>
                {modal?.mode === 'edit' ? 'Update' : 'Create'}
              </Button>
            </div>
          </form>
        </DialogContent>
      </Dialog>

      <ConfirmDeleteModal
        open={!!deleteTarget}
        name={deleteTarget?.name ?? ''}
        onConfirm={() => deleteTarget && deleteMut.mutate(deleteTarget.id)}
        onCancel={() => setDeleteTarget(null)}
        loading={deleteMut.isPending}
      />
    </div>
  );
}
