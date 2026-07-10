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

export default function CurrencyFormPage() {
  const { id } = useParams();
  const isEdit = !!id;
  const navigate = useNavigate();
  const qc = useQueryClient();

  const { data: existing } = useQuery({
    queryKey: ['currencies', id],
    queryFn: () => currencyApi.getById(id!),
    enabled: isEdit,
  });

  const {
    register,
    handleSubmit,
    reset,
    formState: { errors },
  } = useForm<FormData>({
    resolver: zodResolver(schema),
    defaultValues: { isBase: false, isActive: true, exchangeRate: '' },
  });

  useEffect(() => {
    if (existing) {
      reset({
        code: existing.code,
        name: existing.name,
        symbol: existing.symbol ?? '',
        exchangeRate: String(existing.exchangeRate),
        isBase: existing.isBase,
        isActive: existing.isActive,
      });
    }
  }, [existing, reset]);

  const createMut = useMutation({
    mutationFn: currencyApi.create,
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['currencies'] });
      toast.success('Currency created');
      navigate('..');
    },
    onError: (error: unknown) => toast.error(extractApiError(error, 'Create failed')),
  });

  const updateMut = useMutation({
    mutationFn: ({ data }: { data: UpdateCurrencyRequest }) => currencyApi.update(id!, data),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ['currencies'] });
      toast.success('Currency updated');
      navigate('..');
    },
    onError: (error: unknown) => toast.error(extractApiError(error, 'Update failed')),
  });

  const onSubmit = (data: FormData) => {
    const exchangeRate = parseFloat(data.exchangeRate);
    if (isEdit) {
      updateMut.mutate({
        data: {
          code: data.code,
          name: data.name,
          symbol: data.symbol,
          exchangeRate,
          isBase: data.isBase,
          isActive: data.isActive,
        },
      });
    } else {
      createMut.mutate({
        code: data.code,
        name: data.name,
        symbol: data.symbol,
        exchangeRate,
        isBase: data.isBase,
      });
    }
  };

  return (
    <div className="max-w-lg space-y-4">
      <div className="flex items-center gap-2">
        <Button variant="ghost" size="icon" onClick={() => navigate('..')}>
          <ArrowLeft className="h-4 w-4" />
        </Button>
        <h1 className="text-2xl font-bold">{isEdit ? 'Edit Currency' : 'New Currency'}</h1>
      </div>
      <Card>
        <CardContent className="pt-6">
          <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
            <div>
              <Label>Code</Label>
              <Input {...register('code')} placeholder="USD" className="font-mono" />
              {errors.code && (
                <p className="text-sm text-destructive mt-1">{errors.code.message}</p>
              )}
            </div>
            <div>
              <Label>Name</Label>
              <Input {...register('name')} placeholder="US Dollar" />
              {errors.name && (
                <p className="text-sm text-destructive mt-1">{errors.name.message}</p>
              )}
            </div>
            <div>
              <Label>Symbol</Label>
              <Input {...register('symbol')} placeholder="$" />
            </div>
            <div>
              <Label>Exchange Rate (vs IDR)</Label>
              <Input {...register('exchangeRate')} type="number" step="0.000001" />
              {errors.exchangeRate && (
                <p className="text-sm text-destructive mt-1">{errors.exchangeRate.message}</p>
              )}
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
