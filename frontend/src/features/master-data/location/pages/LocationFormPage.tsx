import { useEffect } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { toast } from 'sonner';
import { ArrowLeft } from 'lucide-react';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { Card, CardContent } from '@/components/ui/card';
import { useAuthStore } from '@/stores/authStore';
import { locationApi, type UpdateLocationRequest } from '../api/locationApi';
import { extractApiError } from '@/shared/lib/apiError';

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

export default function LocationFormPage() {
  const { id } = useParams();
  const isEdit = !!id;
  const navigate = useNavigate();
  const qc = useQueryClient();
  const companyId = useAuthStore((s) => s.user?.companyId ?? '');

  const { data: existing } = useQuery({
    queryKey: ['locations', id],
    queryFn: () => locationApi.getById(id!),
    enabled: isEdit,
  });

  const {
    register,
    handleSubmit,
    reset,
    formState: { errors },
  } = useForm<FormData>({
    resolver: zodResolver(schema),
    defaultValues: { type: 'warehouse', country: 'Indonesia', isActive: true },
  });

  useEffect(() => {
    if (existing) {
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
  }, [existing, reset]);

  const createMut = useMutation({
    mutationFn: locationApi.create,
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['locations'] });
      toast.success('Location created');
      navigate('..');
    },
    onError: (error: unknown) => toast.error(extractApiError(error, 'Create failed')),
  });

  const updateMut = useMutation({
    mutationFn: ({ data }: { data: UpdateLocationRequest }) => locationApi.update(id!, data),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['locations'] });
      toast.success('Location updated');
      navigate('..');
    },
    onError: (error: unknown) => toast.error(extractApiError(error, 'Update failed')),
  });

  const onSubmit = (data: FormData) => {
    if (isEdit) {
      updateMut.mutate({ data: data as UpdateLocationRequest });
    } else {
      createMut.mutate({ companyId, ...data });
    }
  };

  return (
    <div className="max-w-lg space-y-4">
      <div className="flex items-center gap-2">
        <Button variant="ghost" size="icon" onClick={() => navigate('..')}>
          <ArrowLeft className="h-4 w-4" />
        </Button>
        <h1 className="text-2xl font-bold">{isEdit ? 'Edit Location' : 'New Location'}</h1>
      </div>
      <Card>
        <CardContent className="pt-6">
          <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
            <div>
              <Label>Name</Label>
              <Input {...register('name')} placeholder="Main Warehouse" />
              {errors.name && (
                <p className="text-sm text-destructive mt-1">{errors.name.message}</p>
              )}
            </div>
            <div>
              <Label>Type</Label>
              <select
                {...register('type')}
                className="flex h-9 w-full rounded-md border border-input bg-transparent px-3 py-1 text-sm shadow-sm transition-colors focus-visible:outline-none focus-visible:ring-1 focus-visible:ring-ring"
              >
                {LOCATION_TYPES.map((t) => (
                  <option key={t} value={t} className="capitalize">
                    {t.charAt(0).toUpperCase() + t.slice(1)}
                  </option>
                ))}
              </select>
              {errors.type && (
                <p className="text-sm text-destructive mt-1">{errors.type.message}</p>
              )}
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
              {errors.country && (
                <p className="text-sm text-destructive mt-1">{errors.country.message}</p>
              )}
            </div>
            {isEdit && (
              <div className="flex items-center gap-2">
                <input type="checkbox" id="isActive" {...register('isActive')} />
                <Label htmlFor="isActive">Active</Label>
              </div>
            )}
            <div className="flex gap-2 pt-2">
              <Button type="submit" disabled={createMut.isPending || updateMut.isPending}>
                {isEdit ? 'Update' : 'Create'}
              </Button>
              <Button type="button" variant="outline" onClick={() => navigate('..')}>
                Cancel
              </Button>
            </div>
          </form>
        </CardContent>
      </Card>
    </div>
  );
}
