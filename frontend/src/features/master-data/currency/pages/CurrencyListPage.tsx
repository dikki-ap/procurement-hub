import { useNavigate } from 'react-router-dom';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { Plus, Pencil, Trash2 } from 'lucide-react';
import { toast } from 'sonner';
import { Button } from '@/components/ui/button';
import { Badge } from '@/components/ui/badge';
import { DataTable, type DataTableColumn } from '@/shared/components/DataTable';
import { currencyApi, type CurrencyDto } from '../api/currencyApi';

const StatusBadge = ({ active }: { active: boolean }) => (
  <span
    className={`inline-flex items-center px-2 py-0.5 rounded-full text-xs font-medium ${
      active ? 'bg-emerald-50 text-emerald-700' : 'bg-slate-100 text-slate-500'
    }`}
  >
    {active ? 'Active' : 'Inactive'}
  </span>
);

export default function CurrencyListPage() {
  const navigate = useNavigate();
  const qc = useQueryClient();

  const { data = [], isLoading } = useQuery({
    queryKey: ['currencies'],
    queryFn: currencyApi.getAll,
  });

  const deleteMut = useMutation({
    mutationFn: currencyApi.delete,
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['currencies'] });
      toast.success('Currency deleted');
    },
    onError: () => toast.error('Delete failed'),
  });

  const columns: DataTableColumn<CurrencyDto>[] = [
    {
      key: 'code',
      header: 'Code',
      sortable: true,
      render: (row) => <span className="font-mono font-medium">{row.code}</span>,
    },
    { key: 'name', header: 'Name', sortable: true },
    {
      key: 'symbol',
      header: 'Symbol',
      render: (row) => row.symbol ?? '-',
    },
    {
      key: 'exchangeRate',
      header: 'Rate (IDR)',
      sortable: true,
      render: (row) => row.exchangeRate.toLocaleString('id-ID'),
    },
    {
      key: 'isBase',
      header: 'Base',
      render: (row) =>
        row.isBase ? (
          <Badge variant="outline" className="text-blue-600 border-blue-200">
            Base
          </Badge>
        ) : null,
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
          <h1 className="text-xl font-semibold text-slate-900">Currencies</h1>
          <p className="text-sm text-slate-500 mt-0.5">Manage exchange rates and base currency</p>
        </div>
        <Button onClick={() => navigate('new')} className="gap-2">
          <Plus className="h-4 w-4" /> Add Currency
        </Button>
      </div>

      <DataTable
        data={data as unknown as Record<string, unknown>[]}
        columns={columns as DataTableColumn<Record<string, unknown>>[]}
        isLoading={isLoading}
        searchPlaceholder="Search currencies..."
        emptyMessage="No currencies found."
        rowActions={(row) => {
          const currency = row as unknown as CurrencyDto;
          return (
            <>
              <Button
                variant="ghost"
                size="icon"
                className="h-8 w-8"
                onClick={() => navigate(currency.id)}
              >
                <Pencil className="h-3.5 w-3.5" />
              </Button>
              <Button
                variant="ghost"
                size="icon"
                className="h-8 w-8 text-red-500 hover:text-red-600"
                onClick={() => {
                  if (confirm('Delete this currency?')) deleteMut.mutate(currency.id);
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
