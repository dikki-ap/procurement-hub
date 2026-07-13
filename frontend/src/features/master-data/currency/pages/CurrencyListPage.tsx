import { useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { Plus, Pencil, Trash2, DollarSign, RefreshCw } from 'lucide-react';
import { toast } from 'sonner';
import { Button } from '@/components/ui/button';
import { Badge } from '@/components/ui/badge';
import { Switch } from '@/components/ui/switch';
import { Label } from '@/components/ui/label';
import { DataTable, type DataTableColumn } from '@/shared/components/DataTable';
import { ConfirmDeleteModal } from '@/shared/components/ConfirmDeleteModal';
import { CurrencyFormModal } from './CurrencyFormModal';
import { currencyApi, type CurrencyDto } from '../api/currencyApi';

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

const formatRate = (rate: number): string => {
  if (rate === 1) return '1';
  if (rate >= 1) return rate.toLocaleString('id-ID', { minimumFractionDigits: 2, maximumFractionDigits: 2 });
  return rate.toLocaleString('id-ID', { maximumSignificantDigits: 4 });
};

const formatRateDate = (value: string | null) => {
  if (!value) return <span className="text-muted-foreground text-xs">—</span>;
  const d = new Date(value);
  return (
    <span className="text-xs text-muted-foreground">
      {d.toLocaleString('en-GB', {
        day: '2-digit', month: 'short', year: 'numeric',
        hour: '2-digit', minute: '2-digit', second: '2-digit',
      })}
    </span>
  );
};

export default function CurrencyListPage() {
  const qc = useQueryClient();
  const [modal, setModal] = useState<ModalState>(null);
  const [deleteTarget, setDeleteTarget] = useState<{ id: string; name: string } | null>(null);

  const { data = [], isLoading } = useQuery({
    queryKey: ['currencies'],
    queryFn: currencyApi.getAll,
  });

  const { data: settings } = useQuery({
    queryKey: ['exchange-rate-settings'],
    queryFn: currencyApi.getExchangeRateSettings,
  });

  const deleteMut = useMutation({
    mutationFn: currencyApi.delete,
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['currencies'] });
      toast.success('Currency deleted', { duration: 3000 });
      setDeleteTarget(null);
    },
    onError: () => {
      toast.error('Delete failed');
      setDeleteTarget(null);
    },
  });

  const syncRatesMut = useMutation({
    mutationFn: currencyApi.syncRates,
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['currencies'] });
      toast.success('Exchange rates refreshed', { duration: 3000 });
    },
    onError: () => toast.error('Rate sync failed — check connection or try again.'),
  });

  const settingsMut = useMutation({
    mutationFn: currencyApi.updateExchangeRateSettings,
    onSuccess: (_, autoSync) => {
      qc.invalidateQueries({ queryKey: ['exchange-rate-settings'] });
      toast.success(autoSync ? 'Auto sync enabled — daily job scheduled.' : 'Manual mode enabled — daily job removed.', { duration: 3000 });
    },
    onError: () => toast.error('Failed to update sync settings.'),
  });

  const autoSync = settings?.autoSync ?? true;

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
      header: 'Rate',
      sortable: true,
      render: (row) => formatRate(row.exchangeRate),
    },
    {
      key: 'rateUpdatedAt',
      header: 'Rate Updated',
      render: (row) => formatRateDate(row.rateUpdatedAt),
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
      <div className="flex flex-wrap items-center justify-between gap-3 mb-6">
        <div className="flex items-center gap-2">
          <DollarSign className="h-5 w-5 text-muted-foreground flex-shrink-0" />
          <div>
            <h1 className="text-xl sm:text-2xl font-semibold">Currencies</h1>
            <p className="text-sm text-muted-foreground hidden sm:block">Manage exchange rates and base currency</p>
          </div>
        </div>
        <div className="flex flex-wrap items-center gap-3">
          <div className="flex items-center gap-2 border rounded-md px-3 py-1.5">
            <Switch
              id="auto-sync"
              checked={autoSync}
              disabled={settingsMut.isPending}
              onCheckedChange={(checked) => settingsMut.mutate(checked)}
            />
            <Label htmlFor="auto-sync" className="text-sm cursor-pointer select-none">
              Auto Sync
            </Label>
            <span className={`text-xs font-medium px-1.5 py-0.5 rounded ${autoSync ? 'bg-emerald-50 text-emerald-700' : 'bg-slate-100 text-slate-500'}`}>
              {autoSync ? 'Daily 09:05 UTC' : 'Manual'}
            </span>
          </div>
          <Button
            size="sm"
            variant="outline"
            className="gap-1"
            disabled={syncRatesMut.isPending}
            onClick={() => syncRatesMut.mutate()}
          >
            <RefreshCw className={`h-4 w-4 ${syncRatesMut.isPending ? 'animate-spin' : ''}`} />
            {syncRatesMut.isPending ? 'Refreshing...' : 'Refresh Rates'}
          </Button>
          <Button size="sm" onClick={() => setModal({ mode: 'add' })} className="gap-1">
            <Plus className="h-4 w-4" /> Add Currency
          </Button>
        </div>
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
                onClick={() => setModal({ mode: 'edit', id: currency.id })}
              >
                <Pencil className="h-3.5 w-3.5" />
              </Button>
              <Button
                variant="ghost"
                size="icon"
                className="h-8 w-8 text-red-500 hover:text-red-600"
                onClick={() => setDeleteTarget({ id: currency.id, name: currency.code })}
              >
                <Trash2 className="h-3.5 w-3.5" />
              </Button>
            </>
          );
        }}
      />

      <CurrencyFormModal
        open={modal !== null}
        id={modal?.mode === 'edit' ? modal.id : undefined}
        onClose={() => setModal(null)}
      />

      <ConfirmDeleteModal
        open={deleteTarget !== null}
        title="Delete Currency"
        description={`Delete "${deleteTarget?.name}"? This action cannot be undone.`}
        isPending={deleteMut.isPending}
        onConfirm={() => deleteTarget && deleteMut.mutate(deleteTarget.id)}
        onCancel={() => setDeleteTarget(null)}
      />
    </div>
  );
}
