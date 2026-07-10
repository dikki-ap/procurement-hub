import { MutationCache, QueryClient } from '@tanstack/react-query';
import { toast } from 'sonner';
import { extractApiError } from './apiError';

export const queryClient = new QueryClient({
  mutationCache: new MutationCache({
    onError: (error, _vars, _ctx, mutation) => {
      // Only fire the global toast if the individual mutation has no onError defined
      if (mutation.options.onError) return;
      toast.error(extractApiError(error));
    },
  }),
  defaultOptions: {
    queries: {
      staleTime: 5 * 60 * 1000,
      retry: 1,
    },
  },
});
