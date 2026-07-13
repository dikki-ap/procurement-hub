import { useEffect, useState } from 'react';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { toast } from 'sonner';
import { AlertTriangle } from 'lucide-react';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
} from '@/components/ui/dialog';
import { currencyApi, type UpdateCurrencyRequest } from '../api/currencyApi';
import { extractApiError } from '@/shared/lib/apiError';

const schema = z.object({
  code: z.string().min(1).max(5),
  name: z.string().min(1).max(50),
  symbol: z.string().max(5).optional(),
  exchangeRate: z.string().min(1),
  isBase: z.boolean(),
  isActive: z.boolean(),
});

type FormData = z.infer<typeof schema>;

const DEFAULTS: FormData = { code: '', name: '', symbol: '', exchangeRate: '', isBase: false, isActive: true };

type Props = {
  open: boolean;
  id?: string;
  onClose: () => void;
};

export function CurrencyFormModal({ open, id, onClose }: Props) {
  const isEdit = !!id;
  const qc = useQueryClient();
  const [pendingData, setPendingData] = useState<FormData | null>(null);

  const { data: existing } = useQuery({
    queryKey: ['currencies', id],
    queryFn: () => currencyApi.getById(id!),
    enabled: isEdit && open,
  });

  const { data: allCurrencies = [] } = useQuery({
    queryKey: ['currencies'],
    queryFn: currencyApi.getAll,
    enabled: open,
  });

  const currentBase = allCurrencies.find((c) => c.isBase);

  const { register, handleSubmit, reset, formState: { errors } } = useForm<FormData>({
    resolver: zodResolver(schema),
    defaultValues: DEFAULTS,
  });

  useEffect(() => {
    if (!open) { reset(DEFAULTS); setPendingData(null); return; }
    if (!id) { reset(DEFAULTS); return; }
  }, [open, id, reset]);

  useEffect(() => {
    if (existing && open) {
      reset({
        code: existing.code,
        name: existing.name,
        symbol: existing.symbol ?? '',
        exchangeRate: String(existing.exchangeRate),
        isBase: existing.isBase,
        isActive: existing.isActive,
      });
    }
  }, [existing, open, reset]);

  const createMut = useMutation({
    mutationFn: currencyApi.create,
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['currencies'] });
      toast.success('Currency created', { duration: 3000 });
      onClose();
    },
    onError: (error: unknown) => toast.error(extractApiError(error, 'Create failed')),
  });

  const updateMut = useMutation({
    mutationFn: ({ data }: { data: UpdateCurrencyRequest }) => currencyApi.update(id!, data),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['currencies'] });
      toast.success('Currency updated', { duration: 3000 });
      onClose();
    },
    onError: (error: unknown) => toast.error(extractApiError(error, 'Update failed')),
  });

  const isPending = createMut.isPending || updateMut.isPending;

  const executeSubmit = (data: FormData) => {
    const exchangeRate = parseFloat(data.exchangeRate);
    if (isEdit) {
      updateMut.mutate({ data: { code: data.code, name: data.name, symbol: data.symbol, exchangeRate, isBase: data.isBase, isActive: data.isActive } });
    } else {
      createMut.mutate({ code: data.code, name: data.name, symbol: data.symbol, exchangeRate, isBase: data.isBase });
    }
  };

  const isChangingBase = (data: FormData) =>
    data.isBase && currentBase !== undefined && currentBase.id !== id;

  const onSubmit = (data: FormData) => {
    if (isChangingBase(data)) {
      setPendingData(data);
    } else {
      executeSubmit(data);
    }
  };

  const newBaseCode = pendingData?.code ?? '';

  return (
    <>
      <Dialog open={open && !pendingData} onOpenChange={(v) => { if (!v && !isPending) onClose(); }}>
        <DialogContent className="max-w-lg">
          <DialogHeader>
            <DialogTitle>{isEdit ? 'Edit Currency' : 'Add Currency'}</DialogTitle>
          </DialogHeader>
          <form onSubmit={handleSubmit(onSubmit)} className="space-y-4 mt-2">
            <div>
              <Label>Code</Label>
              <Input {...register('code')} placeholder="USD" className="font-mono" />
              {errors.code && <p className="text-sm text-destructive mt-1">{errors.code.message}</p>}
            </div>
            <div>
              <Label>Name</Label>
              <Input {...register('name')} placeholder="US Dollar" />
              {errors.name && <p className="text-sm text-destructive mt-1">{errors.name.message}</p>}
            </div>
            <div>
              <Label>Symbol</Label>
              <Input {...register('symbol')} placeholder="$" />
            </div>
            <div>
              <Label>Exchange Rate</Label>
              <Input {...register('exchangeRate')} type="number" step="0.000001" />
              {errors.exchangeRate && <p className="text-sm text-destructive mt-1">{errors.exchangeRate.message}</p>}
            </div>
            <div className="flex items-center gap-2">
              <input type="checkbox" id="isBase" {...register('isBase')} />
              <Label htmlFor="isBase">Base Currency</Label>
            </div>
            {isEdit && (
              <div className="flex items-center gap-2">
                <input type="checkbox" id="isActive" {...register('isActive')} />
                <Label htmlFor="isActive">Active</Label>
              </div>
            )}
            <div className="flex justify-end gap-2 pt-2">
              <Button type="button" variant="outline" onClick={onClose} disabled={isPending}>Cancel</Button>
              <Button type="submit" disabled={isPending}>{isEdit ? 'Update' : 'Create'}</Button>
            </div>
          </form>
        </DialogContent>
      </Dialog>

      <Dialog open={!!pendingData} onOpenChange={(v) => { if (!v) setPendingData(null); }}>
        <DialogContent className="max-w-md">
          <DialogHeader>
            <DialogTitle className="flex items-center gap-2 text-amber-600">
              <AlertTriangle className="h-5 w-5" />
              Change Base Currency?
            </DialogTitle>
          </DialogHeader>
          <div className="space-y-4 mt-2">
            <p className="text-sm text-muted-foreground">
              You are changing the base currency from{' '}
              <span className="font-semibold text-foreground">{currentBase?.code ?? '—'}</span>
              {' '}to{' '}
              <span className="font-semibold text-foreground">{newBaseCode}</span>.
            </p>
            <div className="rounded-md border border-amber-200 bg-amber-50 p-3 text-sm text-amber-800 space-y-1">
              <p className="font-medium">Things that will NOT update automatically:</p>
              <ul className="list-disc list-inside space-y-0.5 text-amber-700">
                <li>Approval policy thresholds (min/max values)</li>
                <li>Existing purchase orders and contracts</li>
              </ul>
              <p className="mt-2">You must manually review and update approval policy amounts after this change.</p>
            </div>
            <div className="flex justify-end gap-2 pt-1">
              <Button
                type="button"
                variant="outline"
                onClick={() => setPendingData(null)}
                disabled={isPending}
              >
                Cancel
              </Button>
              <Button
                type="button"
                variant="destructive"
                disabled={isPending}
                onClick={() => {
                  if (pendingData) executeSubmit(pendingData);
                  setPendingData(null);
                }}
              >
                Yes, change base currency
              </Button>
            </div>
          </div>
        </DialogContent>
      </Dialog>
    </>
  );
}
