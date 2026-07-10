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
import { paymentTermApi, type UpdatePaymentTermRequest } from '../api/paymentTermApi';
import { extractApiError } from '@/shared/lib/apiError';

const schema = z.object({
  code: z.string().min(1).max(20),
  name: z.string().min(1).max(100),
  days: z.string().min(1),
  description: z.string().optional(),
  isActive: z.boolean(),
});

type FormData = z.infer<typeof schema>;

const DEFAULTS: FormData = { code: '', name: '', days: '30', description: '', isActive: true };

type Props = {
  open: boolean;
  id?: string;
  onClose: () => void;
};

export function PaymentTermFormModal({ open, id, onClose }: Props) {
  const isEdit = !!id;
  const qc = useQueryClient();
  const companyId = useAuthStore((s) => s.user?.companyId ?? '');

  const { data: existing } = useQuery({
    queryKey: ['payment-terms', id],
    queryFn: () => paymentTermApi.getById(id!),
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
        days: String(existing.days),
        description: existing.description ?? '',
        isActive: existing.isActive,
      });
    }
  }, [existing, open, reset]);

  const createMut = useMutation({
    mutationFn: paymentTermApi.create,
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['payment-terms'] });
      toast.success('Payment term created', { duration: 3000 });
      onClose();
    },
    onError: (error: unknown) => toast.error(extractApiError(error, 'Create failed')),
  });

  const updateMut = useMutation({
    mutationFn: ({ data }: { data: UpdatePaymentTermRequest }) => paymentTermApi.update(id!, data),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['payment-terms'] });
      toast.success('Payment term updated', { duration: 3000 });
      onClose();
    },
    onError: (error: unknown) => toast.error(extractApiError(error, 'Update failed')),
  });

  const isPending = createMut.isPending || updateMut.isPending;

  const onSubmit = (data: FormData) => {
    const days = parseInt(data.days, 10);
    if (isEdit) {
      updateMut.mutate({ data: { code: data.code, name: data.name, days, description: data.description, isActive: data.isActive } });
    } else {
      createMut.mutate({ companyId, code: data.code, name: data.name, days, description: data.description });
    }
  };

  return (
    <Dialog open={open} onOpenChange={(v) => { if (!v && !isPending) onClose(); }}>
      <DialogContent className="max-w-lg">
        <DialogHeader>
          <DialogTitle>{isEdit ? 'Edit Payment Term' : 'Add Payment Term'}</DialogTitle>
        </DialogHeader>
        <form onSubmit={handleSubmit(onSubmit)} className="space-y-4 mt-2">
          <div>
            <Label>Code</Label>
            <Input {...register('code')} placeholder="NET30" className="font-mono" />
            {errors.code && <p className="text-sm text-destructive mt-1">{errors.code.message}</p>}
          </div>
          <div>
            <Label>Name</Label>
            <Input {...register('name')} placeholder="Net 30 Days" />
            {errors.name && <p className="text-sm text-destructive mt-1">{errors.name.message}</p>}
          </div>
          <div>
            <Label>Days</Label>
            <Input {...register('days')} type="number" min={0} />
            {errors.days && <p className="text-sm text-destructive mt-1">{errors.days.message}</p>}
          </div>
          <div>
            <Label>Description</Label>
            <Input {...register('description')} placeholder="Optional description" />
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
