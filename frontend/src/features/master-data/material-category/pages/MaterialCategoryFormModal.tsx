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
import {
  materialCategoryApi,
  type UpdateMaterialCategoryRequest,
  type MaterialCategoryDto,
} from '../api/materialCategoryApi';
import { extractApiError } from '@/shared/lib/apiError';

const schema = z.object({
  code: z.string().min(1).max(20),
  name: z.string().min(1).max(100),
  parentId: z.string().optional(),
  isStrategic: z.boolean(),
  isActive: z.boolean(),
});

type FormData = z.infer<typeof schema>;

const DEFAULTS: FormData = { code: '', name: '', parentId: '', isStrategic: false, isActive: true };

const SELECT_CLASS = 'flex h-9 w-full rounded-md border border-input bg-transparent px-3 py-1 text-sm shadow-sm transition-colors focus-visible:outline-none focus-visible:ring-1 focus-visible:ring-ring';

type Props = {
  open: boolean;
  id?: string;
  onClose: () => void;
};

export function MaterialCategoryFormModal({ open, id, onClose }: Props) {
  const isEdit = !!id;
  const qc = useQueryClient();
  const companyId = useAuthStore((s) => s.user?.companyId ?? '');

  const { data: existing } = useQuery({
    queryKey: ['material-categories', id],
    queryFn: () => materialCategoryApi.getById(id!),
    enabled: isEdit && open,
  });

  const { data: allCategories = [] } = useQuery({
    queryKey: ['material-categories', companyId],
    queryFn: () => materialCategoryApi.getAll(companyId),
    enabled: !!companyId && open,
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
        parentId: existing.parentId ?? '',
        isStrategic: existing.isStrategic,
        isActive: existing.isActive,
      });
    }
  }, [existing, open, reset]);

  const createMut = useMutation({
    mutationFn: materialCategoryApi.create,
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['material-categories'] });
      toast.success('Category created', { duration: 3000 });
      onClose();
    },
    onError: (error: unknown) => toast.error(extractApiError(error, 'Create failed')),
  });

  const updateMut = useMutation({
    mutationFn: ({ data }: { data: UpdateMaterialCategoryRequest }) =>
      materialCategoryApi.update(id!, data),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['material-categories'] });
      toast.success('Category updated', { duration: 3000 });
      onClose();
    },
    onError: (error: unknown) => toast.error(extractApiError(error, 'Update failed')),
  });

  const isPending = createMut.isPending || updateMut.isPending;

  const onSubmit = (data: FormData) => {
    const parentId = data.parentId || undefined;
    if (isEdit) {
      updateMut.mutate({ data: { code: data.code, name: data.name, parentId, isStrategic: data.isStrategic, isActive: data.isActive } });
    } else {
      createMut.mutate({ companyId, code: data.code, name: data.name, parentId, isStrategic: data.isStrategic });
    }
  };

  const parentOptions = (allCategories as MaterialCategoryDto[]).filter((c) => c.id !== id);

  return (
    <Dialog open={open} onOpenChange={(v) => { if (!v && !isPending) onClose(); }}>
      <DialogContent className="max-w-lg">
        <DialogHeader>
          <DialogTitle>{isEdit ? 'Edit Category' : 'Add Category'}</DialogTitle>
        </DialogHeader>
        <form onSubmit={handleSubmit(onSubmit)} className="space-y-4 mt-2">
          <div>
            <Label>Code</Label>
            <Input {...register('code')} placeholder="CAT-001" className="font-mono" />
            {errors.code && <p className="text-sm text-destructive mt-1">{errors.code.message}</p>}
          </div>
          <div>
            <Label>Name</Label>
            <Input {...register('name')} placeholder="Raw Materials" />
            {errors.name && <p className="text-sm text-destructive mt-1">{errors.name.message}</p>}
          </div>
          <div>
            <Label>Parent Category (optional)</Label>
            <select {...register('parentId')} className={SELECT_CLASS}>
              <option value="">— None —</option>
              {parentOptions.map((c) => (
                <option key={c.id} value={c.id}>{c.name}</option>
              ))}
            </select>
          </div>
          <div className="flex items-center gap-2">
            <input type="checkbox" id="isStrategic" {...register('isStrategic')} />
            <Label htmlFor="isStrategic">Strategic Category</Label>
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
