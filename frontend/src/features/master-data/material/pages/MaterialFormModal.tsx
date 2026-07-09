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
import { useAuthStore } from '@/stores/authStore';
import { materialApi, type UpdateMaterialRequest } from '../api/materialApi';
import { materialCategoryApi, type MaterialCategoryDto } from '@/features/master-data/material-category/api/materialCategoryApi';
import { uomApi, type UomDto } from '@/features/master-data/uom/api/uomApi';
import { currencyApi, type CurrencyDto } from '@/features/master-data/currency/api/currencyApi';

const schema = z.object({
  categoryId: z.string().min(1, 'Category is required'),
  code: z.string().min(1).max(50),
  name: z.string().min(1).max(200),
  description: z.string().optional(),
  uomId: z.string().min(1, 'UOM is required'),
  estimatedPrice: z.string().optional(),
  currencyId: z.string().optional(),
  isStrategic: z.boolean(),
  isActive: z.boolean(),
});

type FormData = z.infer<typeof schema>;

const DEFAULTS: FormData = { categoryId: '', code: '', name: '', description: '', uomId: '', estimatedPrice: '', currencyId: '', isStrategic: false, isActive: true };

const SELECT_CLASS = 'flex h-9 w-full rounded-md border border-input bg-transparent px-3 py-1 text-sm shadow-sm transition-colors focus-visible:outline-none focus-visible:ring-1 focus-visible:ring-ring';

type Props = {
  open: boolean;
  id?: string;
  onClose: () => void;
};

export function MaterialFormModal({ open, id, onClose }: Props) {
  const isEdit = !!id;
  const qc = useQueryClient();
  const companyId = useAuthStore((s) => s.user?.companyId ?? '');

  const { data: existing } = useQuery({
    queryKey: ['materials', id],
    queryFn: () => materialApi.getById(id!),
    enabled: isEdit && open,
  });

  const { data: categories = [] } = useQuery({
    queryKey: ['material-categories', companyId],
    queryFn: () => materialCategoryApi.getAll(companyId),
    enabled: !!companyId && open,
  });

  const { data: uoms = [] } = useQuery({
    queryKey: ['uoms', companyId],
    queryFn: () => uomApi.getAll(companyId),
    enabled: !!companyId && open,
  });

  const { data: currencies = [] } = useQuery({
    queryKey: ['currencies'],
    queryFn: currencyApi.getAll,
    enabled: open,
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
        categoryId: existing.categoryId,
        code: existing.code,
        name: existing.name,
        description: existing.description ?? '',
        uomId: existing.uomId,
        estimatedPrice: existing.estimatedPrice != null ? String(existing.estimatedPrice) : '',
        currencyId: existing.currencyId ?? '',
        isStrategic: existing.isStrategic,
        isActive: existing.isActive,
      });
    }
  }, [existing, open, reset]);

  const createMut = useMutation({
    mutationFn: materialApi.create,
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['materials'] });
      toast.success('Material created', { duration: 3000 });
      onClose();
    },
    onError: () => toast.error('Create failed'),
  });

  const updateMut = useMutation({
    mutationFn: ({ data }: { data: UpdateMaterialRequest }) => materialApi.update(id!, data),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['materials'] });
      toast.success('Material updated', { duration: 3000 });
      onClose();
    },
    onError: () => toast.error('Update failed'),
  });

  const isPending = createMut.isPending || updateMut.isPending;

  const onSubmit = (data: FormData) => {
    const estimatedPrice = data.estimatedPrice ? parseFloat(data.estimatedPrice) : undefined;
    const payload = {
      categoryId: data.categoryId,
      code: data.code,
      name: data.name,
      description: data.description || undefined,
      uomId: data.uomId,
      estimatedPrice,
      currencyId: data.currencyId || undefined,
      isStrategic: data.isStrategic,
    };
    if (isEdit) {
      updateMut.mutate({ data: { ...payload, isActive: data.isActive } });
    } else {
      createMut.mutate(payload);
    }
  };

  return (
    <Dialog open={open} onOpenChange={(v) => { if (!v && !isPending) onClose(); }}>
      <DialogContent className="max-w-lg max-h-[90vh] overflow-y-auto">
        <DialogHeader>
          <DialogTitle>{isEdit ? 'Edit Material' : 'Add Material'}</DialogTitle>
        </DialogHeader>
        <form onSubmit={handleSubmit(onSubmit)} className="space-y-4 mt-2">
          <div>
            <Label>Category</Label>
            <select {...register('categoryId')} className={SELECT_CLASS}>
              <option value="">— Select Category —</option>
              {(categories as MaterialCategoryDto[]).map((c) => (
                <option key={c.id} value={c.id}>{c.name}</option>
              ))}
            </select>
            {errors.categoryId && <p className="text-sm text-destructive mt-1">{errors.categoryId.message}</p>}
          </div>
          <div>
            <Label>Code</Label>
            <Input {...register('code')} placeholder="MAT-001" className="font-mono" />
            {errors.code && <p className="text-sm text-destructive mt-1">{errors.code.message}</p>}
          </div>
          <div>
            <Label>Name</Label>
            <Input {...register('name')} placeholder="Material name" />
            {errors.name && <p className="text-sm text-destructive mt-1">{errors.name.message}</p>}
          </div>
          <div>
            <Label>Description</Label>
            <Input {...register('description')} placeholder="Optional description" />
          </div>
          <div>
            <Label>Unit of Measure</Label>
            <select {...register('uomId')} className={SELECT_CLASS}>
              <option value="">— Select UOM —</option>
              {(uoms as UomDto[]).map((u) => (
                <option key={u.id} value={u.id}>{u.code} — {u.name}</option>
              ))}
            </select>
            {errors.uomId && <p className="text-sm text-destructive mt-1">{errors.uomId.message}</p>}
          </div>
          <div>
            <Label>Estimated Price (optional)</Label>
            <Input {...register('estimatedPrice')} type="number" step="0.01" min={0} />
          </div>
          <div>
            <Label>Currency (optional)</Label>
            <select {...register('currencyId')} className={SELECT_CLASS}>
              <option value="">— None —</option>
              {(currencies as CurrencyDto[]).map((c) => (
                <option key={c.id} value={c.id}>{c.code} — {c.name}</option>
              ))}
            </select>
          </div>
          <div className="flex items-center gap-2">
            <input type="checkbox" id="isStrategic" {...register('isStrategic')} />
            <Label htmlFor="isStrategic">Strategic Material</Label>
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
