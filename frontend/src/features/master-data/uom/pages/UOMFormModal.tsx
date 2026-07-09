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
import { uomApi, type UpdateUomRequest } from '../api/uomApi';

const schema = z.object({
  code: z.string().min(1).max(10),
  name: z.string().min(1).max(50),
  isActive: z.boolean(),
});

type FormData = z.infer<typeof schema>;

const DEFAULTS: FormData = { code: '', name: '', isActive: true };

type Props = {
  open: boolean;
  id?: string;
  onClose: () => void;
};

export function UOMFormModal({ open, id, onClose }: Props) {
  const isEdit = !!id;
  const qc = useQueryClient();
  const companyId = useAuthStore((s) => s.user?.companyId ?? '');

  const { data: existing } = useQuery({
    queryKey: ['uoms', id],
    queryFn: () => uomApi.getById(id!),
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
      reset({ code: existing.code, name: existing.name, isActive: existing.isActive });
    }
  }, [existing, open, reset]);

  const createMut = useMutation({
    mutationFn: uomApi.create,
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['uoms'] });
      toast.success('Unit of measure created', { duration: 3000 });
      onClose();
    },
    onError: () => toast.error('Create failed'),
  });

  const updateMut = useMutation({
    mutationFn: ({ data }: { data: UpdateUomRequest }) => uomApi.update(id!, data),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['uoms'] });
      toast.success('Unit of measure updated', { duration: 3000 });
      onClose();
    },
    onError: () => toast.error('Update failed'),
  });

  const isPending = createMut.isPending || updateMut.isPending;

  const onSubmit = (data: FormData) => {
    if (isEdit) {
      updateMut.mutate({ data });
    } else {
      createMut.mutate({ companyId, code: data.code, name: data.name });
    }
  };

  return (
    <Dialog open={open} onOpenChange={(v) => { if (!v && !isPending) onClose(); }}>
      <DialogContent className="max-w-lg">
        <DialogHeader>
          <DialogTitle>{isEdit ? 'Edit Unit of Measure' : 'Add Unit of Measure'}</DialogTitle>
        </DialogHeader>
        <form onSubmit={handleSubmit(onSubmit)} className="space-y-4 mt-2">
          <div>
            <Label>Code</Label>
            <Input {...register('code')} placeholder="KG" className="font-mono" />
            {errors.code && <p className="text-sm text-destructive mt-1">{errors.code.message}</p>}
          </div>
          <div>
            <Label>Name</Label>
            <Input {...register('name')} placeholder="Kilogram" />
            {errors.name && <p className="text-sm text-destructive mt-1">{errors.name.message}</p>}
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
