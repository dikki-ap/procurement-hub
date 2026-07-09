import { useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { Plus, Pencil, Trash2 } from 'lucide-react';
import { toast } from 'sonner';
import { Button } from '@/components/ui/button';
import { DataTable, type DataTableColumn } from '@/shared/components/DataTable';
import { ConfirmDeleteModal } from '@/shared/components/ConfirmDeleteModal';
import { LocationFormModal } from './LocationFormModal';
import { useAuthStore } from '@/stores/authStore';
import { locationApi, type LocationDto } from '../api/locationApi';

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

export default function LocationListPage() {
  const qc = useQueryClient();
  const companyId = useAuthStore((s) => s.user?.companyId ?? '');
  const [modal, setModal] = useState<ModalState>(null);
  const [deleteTarget, setDeleteTarget] = useState<{ id: string; name: string } | null>(null);

  const { data = [], isLoading } = useQuery({
    queryKey: ['locations', companyId],
    queryFn: () => locationApi.getAll(companyId),
    enabled: !!companyId,
  });

  const deleteMut = useMutation({
    mutationFn: locationApi.delete,
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['locations'] });
      toast.success('Location deleted', { duration: 3000 });
      setDeleteTarget(null);
    },
    onError: () => {
      toast.error('Delete failed');
      setDeleteTarget(null);
    },
  });

  const columns: DataTableColumn<LocationDto>[] = [
    {
      key: 'name',
      header: 'Name',
      sortable: true,
      render: (row) => <span className="font-medium text-slate-900">{row.name}</span>,
    },
    {
      key: 'type',
      header: 'Type',
      sortable: true,
      render: (row) => <span className="capitalize">{row.type}</span>,
    },
    {
      key: 'city',
      header: 'City',
      sortable: true,
      render: (row) => row.city ?? '-',
    },
    {
      key: 'country',
      header: 'Country',
      sortable: true,
      render: (row) => row.country ?? '-',
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
          <h1 className="text-xl font-semibold text-slate-900">Locations</h1>
          <p className="text-sm text-slate-500 mt-0.5">Manage warehouses, plants, and offices</p>
        </div>
        <Button onClick={() => setModal({ mode: 'add' })} className="gap-2">
          <Plus className="h-4 w-4" /> Add Location
        </Button>
      </div>

      <DataTable
        data={data as unknown as Record<string, unknown>[]}
        columns={columns as DataTableColumn<Record<string, unknown>>[]}
        isLoading={isLoading}
        searchPlaceholder="Search locations..."
        emptyMessage="No locations found."
        rowActions={(row) => {
          const location = row as unknown as LocationDto;
          return (
            <>
              <Button
                variant="ghost"
                size="icon"
                className="h-8 w-8"
                onClick={() => setModal({ mode: 'edit', id: location.id })}
              >
                <Pencil className="h-3.5 w-3.5" />
              </Button>
              <Button
                variant="ghost"
                size="icon"
                className="h-8 w-8 text-red-500 hover:text-red-600"
                onClick={() => setDeleteTarget({ id: location.id, name: location.name })}
              >
                <Trash2 className="h-3.5 w-3.5" />
              </Button>
            </>
          );
        }}
      />

      <LocationFormModal
        open={modal !== null}
        id={modal?.mode === 'edit' ? modal.id : undefined}
        onClose={() => setModal(null)}
      />

      <ConfirmDeleteModal
        open={deleteTarget !== null}
        title="Delete Location"
        description={`Delete "${deleteTarget?.name}"? This action cannot be undone.`}
        isPending={deleteMut.isPending}
        onConfirm={() => deleteTarget && deleteMut.mutate(deleteTarget.id)}
        onCancel={() => setDeleteTarget(null)}
      />
    </div>
  );
}
