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
import { locationApi, type UpdateLocationRequest } from '../api/locationApi';

const LOCATION_TYPES = ['warehouse', 'plant', 'office'] as const;

const schema = z.object({
  name: z.string().min(1).max(100),
  type: z.enum(LOCATION_TYPES),
  address: z.string().optional(),
  city: z.string().optional(),
  province: z.string().optional(),
  country: z.string().min(1).max(100),
  isActive: z.boolean(),
});

type FormData = z.infer<typeof schema>;

const DEFAULTS: FormData = { name: '', type: 'warehouse', address: '', city: '', province: '', country: 'Indonesia', isActive: true };

const SELECT_CLASS = 'flex h-9 w-full rounded-md border border-input bg-transparent px-3 py-1 text-sm shadow-sm transition-colors focus-visible:outline-none focus-visible:ring-1 focus-visible:ring-ring';

type Props = {
  open: boolean;
  id?: string;
  onClose: () => void;
};

export function LocationFormModal({ open, id, onClose }: Props) {
  const isEdit = !!id;
  const qc = useQueryClient();
  const companyId = useAuthStore((s) => s.user?.companyId ?? '');

  const { data: existing } = useQuery({
    queryKey: ['locations', id],
    queryFn: () => locationApi.getById(id!),
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
        name: existing.name,
        type: existing.type as (typeof LOCATION_TYPES)[number],
        address: existing.address ?? '',
        city: existing.city ?? '',
        province: existing.province ?? '',
        country: existing.country,
        isActive: existing.isActive,
      });
    }
  }, [existing, open, reset]);

  const createMut = useMutation({
    mutationFn: locationApi.create,
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['locations'] });
      toast.success('Location created', { duration: 3000 });
      onClose();
    },
    onError: () => toast.error('Create failed'),
  });

  const updateMut = useMutation({
    mutationFn: ({ data }: { data: UpdateLocationRequest }) => locationApi.update(id!, data),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['locations'] });
      toast.success('Location updated', { duration: 3000 });
      onClose();
    },
    onError: () => toast.error('Update failed'),
  });

  const isPending = createMut.isPending || updateMut.isPending;

  const onSubmit = (data: FormData) => {
    if (isEdit) {
      updateMut.mutate({ data: data as UpdateLocationRequest });
    } else {
      createMut.mutate({ companyId, ...data });
    }
  };

  return (
    <Dialog open={open} onOpenChange={(v) => { if (!v && !isPending) onClose(); }}>
      <DialogContent className="max-w-lg max-h-[90vh] overflow-y-auto">
        <DialogHeader>
          <DialogTitle>{isEdit ? 'Edit Location' : 'Add Location'}</DialogTitle>
        </DialogHeader>
        <form onSubmit={handleSubmit(onSubmit)} className="space-y-4 mt-2">
          <div>
            <Label>Name</Label>
            <Input {...register('name')} placeholder="Main Warehouse" />
            {errors.name && <p className="text-sm text-destructive mt-1">{errors.name.message}</p>}
          </div>
          <div>
            <Label>Type</Label>
            <select {...register('type')} className={SELECT_CLASS}>
              {LOCATION_TYPES.map((t) => (
                <option key={t} value={t}>{t.charAt(0).toUpperCase() + t.slice(1)}</option>
              ))}
            </select>
            {errors.type && <p className="text-sm text-destructive mt-1">{errors.type.message}</p>}
          </div>
          <div>
            <Label>Address</Label>
            <Input {...register('address')} placeholder="Street address" />
          </div>
          <div>
            <Label>City</Label>
            <Input {...register('city')} placeholder="Jakarta" />
          </div>
          <div>
            <Label>Province</Label>
            <Input {...register('province')} placeholder="DKI Jakarta" />
          </div>
          <div>
            <Label>Country</Label>
            <Input {...register('country')} placeholder="Indonesia" />
            {errors.country && <p className="text-sm text-destructive mt-1">{errors.country.message}</p>}
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
