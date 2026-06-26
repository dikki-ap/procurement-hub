import { useNavigate } from 'react-router-dom';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { Plus, Pencil, Trash2 } from 'lucide-react';
import { toast } from 'sonner';
import { Button } from '@/components/ui/button';
import { DataTable, type DataTableColumn } from '@/shared/components/DataTable';
import { useAuthStore } from '@/stores/authStore';
import { uomApi, type UomDto } from '../api/uomApi';

const StatusBadge = ({ active }: { active: boolean }) => (
  <span
    className={`inline-flex items-center px-2 py-0.5 rounded-full text-xs font-medium ${
      active ? 'bg-emerald-50 text-emerald-700' : 'bg-slate-100 text-slate-500'
    }`}
  >
    {active ? 'Active' : 'Inactive'}
  </span>
);

export default function UOMListPage() {
  const navigate = useNavigate();
  const qc = useQueryClient();
  const companyId = useAuthStore((s) => s.user?.companyId ?? '');

  const { data = [], isLoading } = useQuery({
    queryKey: ['uoms', companyId],
    queryFn: () => uomApi.getAll(companyId),
    enabled: !!companyId,
  });

  const deleteMut = useMutation({
    mutationFn: uomApi.delete,
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['uoms'] });
      toast.success('Unit of measure deleted');
    },
    onError: () => toast.error('Delete failed'),
  });

  const columns: DataTableColumn<UomDto>[] = [
    {
      key: 'code',
      header: 'Code',
      sortable: true,
      render: (row) => <span className="font-mono font-medium">{row.code}</span>,
    },
    { key: 'name', header: 'Name', sortable: true },
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
          <h1 className="text-xl font-semibold text-slate-900">Units of Measure</h1>
          <p className="text-sm text-slate-500 mt-0.5">Manage measurement units for materials</p>
        </div>
        <Button onClick={() => navigate('new')} className="gap-2">
          <Plus className="h-4 w-4" /> Add UOM
        </Button>
      </div>

      <DataTable
        data={data as unknown as Record<string, unknown>[]}
        columns={columns as DataTableColumn<Record<string, unknown>>[]}
        isLoading={isLoading}
        searchPlaceholder="Search units of measure..."
        emptyMessage="No units of measure found."
        rowActions={(row) => {
          const uom = row as unknown as UomDto;
          return (
            <>
              <Button
                variant="ghost"
                size="icon"
                className="h-8 w-8"
                onClick={() => navigate(uom.id)}
              >
                <Pencil className="h-3.5 w-3.5" />
              </Button>
              <Button
                variant="ghost"
                size="icon"
                className="h-8 w-8 text-red-500 hover:text-red-600"
                onClick={() => {
                  if (confirm('Delete this unit of measure?')) deleteMut.mutate(uom.id);
                }}
              >
                <Trash2 className="h-3.5 w-3.5" />
              </Button>
            </>
          );
        }}
      />
    </div>
  );
}
