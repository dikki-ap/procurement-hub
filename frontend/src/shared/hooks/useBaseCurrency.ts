import { useQuery } from '@tanstack/react-query';
import { currencyApi } from '@/features/master-data/currency/api/currencyApi';

export function useBaseCurrency() {
  const { data = [] } = useQuery({
    queryKey: ['currencies'],
    queryFn: currencyApi.getAll,
    staleTime: 10 * 60 * 1000,
  });
  return data.find(c => c.isBase) ?? null;
}
