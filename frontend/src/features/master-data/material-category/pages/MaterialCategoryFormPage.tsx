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

export default function MaterialCategoryFormPage() {
  const { id } = useParams();
  const isEdit = !!id;
  const navigate = useNavigate();
  const qc = useQueryClient();
  const companyId = useAuthStore((s) => s.user?.companyId ?? '');

  const { data: existing } = useQuery({
    queryKey: ['material-categories', id],
    queryFn: () => materialCategoryApi.getById(id!),
    enabled: isEdit,
  });

  const { data: allCategories = [] } = useQuery({
    queryKey: ['material-categories', companyId],
    queryFn: () => materialCategoryApi.getAll(companyId),
    enabled: !!companyId,
  });

  const {
    register,
    handleSubmit,
    reset,
    formState: { errors },
  } = useForm<FormData>({
    resolver: zodResolver(schema),
    defaultValues: { isStrategic: false, isActive: true },
  });

  useEffect(() => {
    if (existing) {
      reset({
        code: existing.code,
        name: existing.name,
        parentId: existing.parentId ?? '',
        isStrategic: existing.isStrategic,
        isActive: existing.isActive,
      });
    }
  }, [existing, reset]);

  const createMut = useMutation({
    mutationFn: materialCategoryApi.create,
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['material-categories'] });
      toast.success('Category created');
      navigate('..');
    },
    onError: (error: unknown) => toast.error(extractApiError(error, 'Create failed')),
  });

  const updateMut = useMutation({
    mutationFn: ({ data }: { data: UpdateMaterialCategoryRequest }) =>
      materialCategoryApi.update(id!, data),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['material-categories'] });
      toast.success('Category updated');
      navigate('..');
    },
    onError: (error: unknown) => toast.error(extractApiError(error, 'Update failed')),
  });

  const onSubmit = (data: FormData) => {
    const parentId = data.parentId || undefined;
    if (isEdit) {
      updateMut.mutate({
        data: {
          code: data.code,
          name: data.name,
          parentId,
          isStrategic: data.isStrategic,
          isActive: data.isActive,
        },
      });
    } else {
      createMut.mutate({ companyId, code: data.code, name: data.name, parentId, isStrategic: data.isStrategic });
    }
  };

  const parentOptions = allCategories.filter(
    (c: MaterialCategoryDto) => c.id !== id
  );

  return (
    <div className="max-w-lg space-y-4">
      <div className="flex items-center gap-2">
        <Button variant="ghost" size="icon" onClick={() => navigate('..')}>
          <ArrowLeft className="h-4 w-4" />
        </Button>
        <h1 className="text-2xl font-bold">
          {isEdit ? 'Edit Category' : 'New Category'}
        </h1>
      </div>
      <Card>
        <CardContent className="pt-6">
          <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
            <div>
              <Label>Code</Label>
              <Input {...register('code')} placeholder="CAT-001" className="font-mono" />
              {errors.code && (
                <p className="text-sm text-destructive mt-1">{errors.code.message}</p>
              )}
            </div>
            <div>
              <Label>Name</Label>
              <Input {...register('name')} placeholder="Raw Materials" />
              {errors.name && (
                <p className="text-sm text-destructive mt-1">{errors.name.message}</p>
              )}
            </div>
            <div>
              <Label>Parent Category (optional)</Label>
              <select
                {...register('parentId')}
                className="flex h-9 w-full rounded-md border border-input bg-transparent px-3 py-1 text-sm shadow-sm transition-colors focus-visible:outline-none focus-visible:ring-1 focus-visible:ring-ring"
              >
                <option value="">— None —</option>
                {parentOptions.map((c: MaterialCategoryDto) => (
                  <option key={c.id} value={c.id}>
                    {c.name}
                  </option>
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
