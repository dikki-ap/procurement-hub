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

export default function PaymentTermFormPage() {
  const { id } = useParams();
  const isEdit = !!id;
  const navigate = useNavigate();
  const qc = useQueryClient();
  const companyId = useAuthStore((s) => s.user?.companyId ?? '');

  const { data: existing } = useQuery({
    queryKey: ['payment-terms', id],
    queryFn: () => paymentTermApi.getById(id!),
    enabled: isEdit,
  });

  const {
    register,
    handleSubmit,
    reset,
    formState: { errors },
  } = useForm<FormData>({
    resolver: zodResolver(schema),
    defaultValues: { isActive: true, days: '30' },
  });

  useEffect(() => {
    if (existing) {
      reset({
        code: existing.code,
        name: existing.name,
        days: String(existing.days),
        description: existing.description ?? '',
        isActive: existing.isActive,
      });
    }
  }, [existing, reset]);

  const createMut = useMutation({
    mutationFn: paymentTermApi.create,
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['payment-terms'] });
      toast.success('Payment term created');
      navigate('..');
    },
    onError: (error: unknown) => toast.error(extractApiError(error, 'Create failed')),
  });

  const updateMut = useMutation({
    mutationFn: ({ data }: { data: UpdatePaymentTermRequest }) =>
      paymentTermApi.update(id!, data),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['payment-terms'] });
      toast.success('Payment term updated');
      navigate('..');
    },
    onError: (error: unknown) => toast.error(extractApiError(error, 'Update failed')),
  });

  const onSubmit = (data: FormData) => {
    const days = parseInt(data.days, 10);
    if (isEdit) {
      updateMut.mutate({
        data: {
          code: data.code,
          name: data.name,
          days,
          description: data.description,
          isActive: data.isActive,
        },
      });
    } else {
      createMut.mutate({
        companyId,
        code: data.code,
        name: data.name,
        days,
        description: data.description,
      });
    }
  };

  return (
    <div className="max-w-lg space-y-4">
      <div className="flex items-center gap-2">
        <Button variant="ghost" size="icon" onClick={() => navigate('..')}>
          <ArrowLeft className="h-4 w-4" />
        </Button>
        <h1 className="text-2xl font-bold">
          {isEdit ? 'Edit Payment Term' : 'New Payment Term'}
        </h1>
      </div>
      <Card>
        <CardContent className="pt-6">
          <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
            <div>
              <Label>Code</Label>
              <Input {...register('code')} placeholder="NET30" className="font-mono" />
              {errors.code && (
                <p className="text-sm text-destructive mt-1">{errors.code.message}</p>
              )}
            </div>
            <div>
              <Label>Name</Label>
              <Input {...register('name')} placeholder="Net 30 Days" />
              {errors.name && (
                <p className="text-sm text-destructive mt-1">{errors.name.message}</p>
              )}
            </div>
            <div>
              <Label>Days</Label>
              <Input {...register('days')} type="number" min={0} />
              {errors.days && (
                <p className="text-sm text-destructive mt-1">{errors.days.message}</p>
              )}
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
