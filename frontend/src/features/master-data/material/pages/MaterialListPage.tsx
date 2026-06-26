import { useNavigate } from 'react-router-dom';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { Plus, Pencil, Trash2 } from 'lucide-react';
import { toast } from 'sonner';
import { Button } from '@/components/ui/button';
import { DataTable, type DataTableColumn } from '@/shared/components/DataTable';
import { materialApi, type MaterialDto } from '../api/materialApi';

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
  const navigate = useNavigate();
  const qc = useQueryClient();

  const { data = [], isLoading } = useQuery({
    queryKey: ['materials'],
    queryFn: materialApi.getAll,
  });

  const deleteMut = useMutation({
    mutationFn: materialApi.delete,
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['materials'] });
      toast.success('Material deleted');
    },
    onError: () => toast.error('Delete failed'),
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
  ];

  return (
    <div>
      <div className="flex items-center justify-between mb-6">
        <div>
          <h1 className="text-xl font-semibold text-slate-900">Materials</h1>
          <p className="text-sm text-slate-500 mt-0.5">Manage your material master data catalog</p>
        </div>
        <Button onClick={() => navigate('new')} className="gap-2">
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
                onClick={() => navigate(material.id)}
              >
                <Pencil className="h-3.5 w-3.5" />
              </Button>
              <Button
                variant="ghost"
                size="icon"
                className="h-8 w-8 text-red-500 hover:text-red-600"
                onClick={() => {
                  if (confirm('Delete this material?')) deleteMut.mutate(material.id);
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
