import { useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { Plus, Pencil, Trash2, FileText } from 'lucide-react';
import { toast } from 'sonner';
import { Button } from '@/components/ui/button';
import { DataTable, type DataTableColumn } from '@/shared/components/DataTable';
import { ConfirmDeleteModal } from '@/shared/components/ConfirmDeleteModal';
import { DocumentTypeFormModal } from './DocumentTypeFormModal';
import { documentTypeApi, type DocumentTypeDto } from '../api/documentTypeApi';
import { extractApiError } from '@/shared/lib/apiError';

type ModalState = { mode: 'add' | 'edit'; id?: string } | null;

const StatusBadge = ({ active }: { active: boolean }) => (
  <span className={`inline-flex items-center px-2 py-0.5 rounded-full text-xs font-medium ${active ? 'bg-emerald-50 text-emerald-700' : 'bg-slate-100 text-slate-500'}`}>
    {active ? 'Active' : 'Inactive'}
  </span>
);

export default function DocumentTypeListPage() {
  const qc = useQueryClient();
  const [modal, setModal] = useState<ModalState>(null);
  const [deleteTarget, setDeleteTarget] = useState<{ id: string; name: string } | null>(null);

  const { data = [], isLoading } = useQuery({
    queryKey: ['document-types'],
    queryFn: documentTypeApi.getAll,
  });

  const deleteMut = useMutation({
    mutationFn: documentTypeApi.delete,
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['document-types'] });
      toast.success('Document type deleted');
      setDeleteTarget(null);
    },
    onError: (e: unknown) => toast.error(extractApiError(e, 'Delete failed')),
  });

  const columns: DataTableColumn<DocumentTypeDto>[] = [
    {
      key: 'name',
      header: 'Name',
      sortable: true,
      render: (row) => <span className="font-medium font-mono">{row.name}</span>,
    },
    {
      key: 'allowedExtensions',
      header: 'Allowed Extensions',
      render: (row) => (
        <span className="text-xs text-slate-500">
          {row.allowedExtensions ?? 'All types'}
        </span>
      ),
    },
    {
      key: 'maxFileSizeMb',
      header: 'Max Size',
      render: (row) => <span className="text-xs text-slate-600">{row.maxFileSizeMb} MB</span>,
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
          <FileText className="h-5 w-5 text-muted-foreground flex-shrink-0" />
          <div>
            <h1 className="text-xl sm:text-2xl font-semibold">Document Types</h1>
            <p className="text-sm text-muted-foreground hidden sm:block">Manage vendor document categories and upload constraints</p>
          </div>
        </div>
        <Button size="sm" onClick={() => setModal({ mode: 'add' })} className="gap-1">
          <Plus className="h-4 w-4" /> Add Document Type
        </Button>
      </div>

      <DataTable
        data={data as unknown as Record<string, unknown>[]}
        columns={columns as DataTableColumn<Record<string, unknown>>[]}
        isLoading={isLoading}
        searchPlaceholder="Search document types..."
        emptyMessage="No document types found."
        rowActions={(row) => {
          const dt = row as unknown as DocumentTypeDto;
          return (
            <>
              <Button variant="ghost" size="icon" className="h-8 w-8" onClick={() => setModal({ mode: 'edit', id: dt.id })}>
                <Pencil className="h-3.5 w-3.5" />
              </Button>
              <Button
                variant="ghost" size="icon" className="h-8 w-8 text-red-500 hover:text-red-600"
                onClick={() => setDeleteTarget({ id: dt.id, name: dt.name })}
              >
                <Trash2 className="h-3.5 w-3.5" />
              </Button>
            </>
          );
        }}
      />

      <DocumentTypeFormModal
        open={modal !== null}
        id={modal?.mode === 'edit' ? modal.id : undefined}
        onClose={() => setModal(null)}
      />

      <ConfirmDeleteModal
        open={deleteTarget !== null}
        title="Delete Document Type"
        description={`Delete "${deleteTarget?.name}"? This action cannot be undone.`}
        isPending={deleteMut.isPending}
        onConfirm={() => deleteTarget && deleteMut.mutate(deleteTarget.id)}
        onCancel={() => setDeleteTarget(null)}
      />
    </div>
  );
}
