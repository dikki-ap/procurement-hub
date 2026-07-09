import { useEffect } from 'react';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { toast } from 'sonner';
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

  const { data: existing } = useQuery({
    queryKey: ['currencies', id],
    queryFn: () => currencyApi.getById(id!),
    enabled: isEdit && open,
  });

  const { register, handleSubmit, reset, formState: { errors } } = useForm<FormData>({
    resolver: zodResolver(schema),
    defaultValues: DEFAULTS,
  });

  useEffect(() => {
    if (!open) { reset(DEFAULTS); return; }
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
    onError: () => toast.error('Create failed'),
  });

  const updateMut = useMutation({
    mutationFn: ({ data }: { data: UpdateCurrencyRequest }) => currencyApi.update(id!, data),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['currencies'] });
      toast.success('Currency updated', { duration: 3000 });
      onClose();
    },
    onError: () => toast.error('Update failed'),
  });

  const isPending = createMut.isPending || updateMut.isPending;

  const onSubmit = (data: FormData) => {
    const exchangeRate = parseFloat(data.exchangeRate);
    if (isEdit) {
      updateMut.mutate({ data: { code: data.code, name: data.name, symbol: data.symbol, exchangeRate, isBase: data.isBase, isActive: data.isActive } });
    } else {
      createMut.mutate({ code: data.code, name: data.name, symbol: data.symbol, exchangeRate, isBase: data.isBase });
    }
  };

  return (
    <Dialog open={open} onOpenChange={(v) => { if (!v && !isPending) onClose(); }}>
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
            <Label>Exchange Rate (vs IDR)</Label>
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
  );
}
